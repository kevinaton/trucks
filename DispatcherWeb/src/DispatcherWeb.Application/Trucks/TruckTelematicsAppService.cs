using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Threading;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.BackgroundJobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker;
using DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using DispatcherWeb.Trucks.Dto;
using Microsoft.EntityFrameworkCore;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TruckPosition = DispatcherWeb.TruckPositions.TruckPosition;

namespace DispatcherWeb.Trucks
{
    public class TruckTelematicsAppService : DispatcherWebAppServiceBase, ITruckTelematicsAppService
    {
        private readonly ITelematics _geotabTelematics;
        private readonly ISamsaraTelematics _samsaraTelematics;
        private readonly IDtdTrackerTelematics _dtdTrackerTelematics;
        private readonly IIntelliShiftTelematics _intelliShiftTelematics;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<WialonDeviceType, long> _wialonDeviceTypeRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<TruckPosition> _truckPositionRepository;
        private readonly ITruckAppService _truckAppService;

        public TruckTelematicsAppService(
            ITelematics geotabTelematics,
            ISamsaraTelematics samsaraTelematics,
            IDtdTrackerTelematics dtdTrackerTelematics,
            IIntelliShiftTelematics intelliShiftTelematics,
            INotificationPublisher notificationPublisher,
            IBackgroundJobManager backgroundJobManager,
            IRepository<Truck> truckRepository,
            IRepository<VehicleUsage> vehicleUsageRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<WialonDeviceType, long> wialonDeviceTypeRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<TruckPosition> truckPositionRepository,
            ITruckAppService truckAppService
        )
        {
            _geotabTelematics = geotabTelematics;
            _samsaraTelematics = samsaraTelematics;
            _dtdTrackerTelematics = dtdTrackerTelematics;
            _intelliShiftTelematics = intelliShiftTelematics;
            _notificationPublisher = notificationPublisher;
            _backgroundJobManager = backgroundJobManager;
            _truckRepository = truckRepository;
            _vehicleUsageRepository = vehicleUsageRepository;
            _tenantRepository = tenantRepository;
            _wialonDeviceTypeRepository = wialonDeviceTypeRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _truckPositionRepository = truckPositionRepository;
            _truckAppService = truckAppService;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<bool> ScheduleUpdateMileage()
        {
            Debug.Assert(AbpSession.TenantId != null, "AbpSession.TenantId != null");
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }

            try
            {
                if (!await IsGpsIntegrationConfigured())
                {
                    throw new UserFriendlyException(L("GpsIntegrationSettingsAreEmptyError"));
                }
                if (await SettingManager.GetGpsPlatformAsync() == GpsPlatform.Geotab)
                {
                    await _geotabTelematics.CheckCredentialsAsync();
                }
            }
            catch (UserFriendlyException e)
            {
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.MileageUpdateError,
                    new MessageNotificationData($"Update mileage failed. {e.Message}"),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[] { new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value) }
                );
                return false;
            }

            await _backgroundJobManager.EnqueueAsync<UpdateMileageJob, UpdateMileageJobArgs>(new UpdateMileageJobArgs()
            {
                TenantId = AbpSession.TenantId.Value,
                UserId = AbpSession.UserId.Value,
            });
            return true;
        }

        [RemoteService(false)]
        [UnitOfWork]
        public async Task<(int trucksUpdated, int trucksIgnored)> UpdateMileageForTenantAsync(int tenantId, long userId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            using (AbpSession.Use(tenantId, userId))
            {
                return await UpdateMileageForCurrentTenantAsync();
            }
        }

        private async Task<(int trucksUpdated, int trucksIgnored)> UpdateMileageForCurrentTenantAsync(bool continueOnError = false)
        {
            (int trucksUpdated, int trucksIgnored) result = (0, 0);
            var truckCurrentData = new List<TruckCurrentData>();

            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return result;
            }

            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();

            if (gpsPlatform == GpsPlatform.Geotab && !(await SettingManager.GetGeotabSettingsAsync()).IsEmpty())
            {
                try
                {
                    var geotabData = await _geotabTelematics.GetCurrentDataForAllTrucksAsync();
                    truckCurrentData.AddRange(geotabData);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString(), e);
                    if (!continueOnError)
                    {
                        throw;
                    }
                }
            }

            if (gpsPlatform == GpsPlatform.Samsara && !(await SettingManager.GetSamsaraSettingsAsync()).IsEmpty())
            {
                try
                {
                    var samsaraData = await _samsaraTelematics.GetCurrentDataForAllTrucksAsync();
                    truckCurrentData.AddRange(samsaraData);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString(), e);
                    if (!continueOnError)
                    {
                        throw;
                    }
                }
            }

            if (gpsPlatform == GpsPlatform.DtdTracker && !(await SettingManager.GetDtdTrackerSettingsAsync()).IsEmpty())
            {
                try
                {
                    var dtdData = await _dtdTrackerTelematics.GetCurrentDataForAllTrucksAsync();
                    truckCurrentData.AddRange(dtdData);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString(), e);
                    if (!continueOnError)
                    {
                        throw;
                    }
                }
            }

            if (gpsPlatform == GpsPlatform.IntelliShift && !(await SettingManager.GetIntelliShiftSettingsAsync()).IsEmpty())
            {
                try
                {
                    var intelliShiftData = await _intelliShiftTelematics.GetCurrentDataForAllTrucksAsync();
                    truckCurrentData.AddRange(intelliShiftData);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString(), e);
                    if (!continueOnError)
                    {
                        throw;
                    }
                }
            }

            if (gpsPlatform == GpsPlatform.DtdTracker)
            {
                var truckUniqueIds = truckCurrentData.Select(x => x.TruckCodeOrUniqueId).Distinct().ToList();
                var trucks = await _truckRepository.GetAll().Where(t => truckUniqueIds.Contains(t.DtdTrackerUniqueId)).ToListAsync();
                foreach (TruckCurrentData currentData in truckCurrentData)
                {
                    var truck = trucks.FirstOrDefault(t => t.DtdTrackerUniqueId == currentData.TruckCodeOrUniqueId);
                    if (truck != null)
                    {
                        result.trucksUpdated++;
                        truck.CurrentMileage = (int)currentData.CurrentMileage;
                        truck.CurrentHours = (decimal)currentData.CurrentHours;
                        Logger.Info(
                            $"Truck with DtdTrackerUniqueId='{currentData.TruckCodeOrUniqueId}' is updated with mileage='{(int)currentData.CurrentMileage}, hours='{currentData.CurrentHours}'.");
                        AddVehicleUsageRecord(truck.Id, currentData.CurrentMileage, currentData.CurrentHours);
                    }
                    else
                    {
                        result.trucksIgnored++;
                        Logger.Warn($"Truck with DtdTrackerUniqueId='{currentData.TruckCodeOrUniqueId}' is not found in the Database.");
                    }
                }
            }
            else
            {
                var truckCodes = truckCurrentData.Select(x => x.TruckCodeOrUniqueId).Distinct().ToList();
                var trucks = await _truckRepository.GetAll().Where(t => truckCodes.Contains(t.TruckCode)).ToListAsync();
                foreach (TruckCurrentData currentData in truckCurrentData)
                {
                    Truck truck = trucks.FirstOrDefault(t => t.TruckCode == currentData.TruckCodeOrUniqueId);
                    if (truck != null)
                    {
                        result.trucksUpdated++;
                        truck.CurrentMileage = (int)currentData.CurrentMileage;
                        truck.CurrentHours = (decimal)currentData.CurrentHours;
                        Logger.Info(
                            $"Truck with TruckCode='{currentData.TruckCodeOrUniqueId}' is updated with mileage='{(int)currentData.CurrentMileage}, hours='{currentData.CurrentHours}'.");
                        AddVehicleUsageRecord(truck.Id, currentData.CurrentMileage, currentData.CurrentHours);
                    }
                    else
                    {
                        result.trucksIgnored++;
                        Logger.Warn($"Truck with TruckCode='{currentData.TruckCodeOrUniqueId}' is not found in the Database.");
                    }
                }
            }


            await CurrentUnitOfWork.SaveChangesAsync();

            return result;

            // Local functions
            void AddVehicleUsageRecord(int truckId, double mileage, double hours)
            {
                DateTime readingDateTime = Clock.Now;
                _vehicleUsageRepository.Insert(new VehicleUsage()
                {
                    TruckId = truckId,
                    ReadingDateTime = readingDateTime,
                    ReadingType = ReadingType.Miles,
                    Reading = (decimal)mileage,
                });
                _vehicleUsageRepository.Insert(new VehicleUsage()
                {
                    TruckId = truckId,
                    ReadingDateTime = readingDateTime,
                    ReadingType = ReadingType.Hours,
                    Reading = (decimal)hours,
                });
            }
        }

        [AbpAuthorize]
        public async Task<string> TestDtd()
        {
            return await _dtdTrackerTelematics.TestDtd();
        }

        [AbpAuthorize]
        public async Task<PagedResultDto<SelectListDto>> GetWialonDeviceTypesSelectList(GetSelectListInput input)
        {
            var query = _wialonDeviceTypeRepository.GetAll();

            return await query
                .Select(x => new SelectListDto<WialonDeviceTypeSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new WialonDeviceTypeSelectListInfoDto
                    {
                        ServerAddress = x.ServerAddress
                    }
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize]
        public async Task SyncWialonDeviceTypes()
        {
            await SyncWialonDeviceTypesInternal();
        }

        [RemoteService(false)]
        [UnitOfWork]
        public async Task SyncWialonDeviceTypesInternal()
        {
            Logger.Info("Updating Wialon Device Types");
            var apiDeviceTypes = await _dtdTrackerTelematics.GetDeviceTypes();
            var localDeviceTypes = await _wialonDeviceTypeRepository.GetAll().ToListAsync();
            var localItemsToDetele = localDeviceTypes.Where(l => !apiDeviceTypes.Items.Any(a => a.Id == l.Id)).ToList();
            if (localItemsToDetele.Any())
            {
                localItemsToDetele.ForEach(_wialonDeviceTypeRepository.Delete);
                Logger.Info($"Deleted {localItemsToDetele.Count} wialon device types: " + string.Join(",", localItemsToDetele.Select(x => $"{x.Id}:{x.Name}")));
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            var addedCount = 0;
            var updatedCount = 0;
            var i = 0;
            foreach (var apiItem in apiDeviceTypes.Items)
            {
                var itemIsNew = false;
                var itemWasUpdated = false;
                var localItem = localDeviceTypes.FirstOrDefault(x => x.Id == apiItem.Id);
                if (localItem == null)
                {
                    localItem = new WialonDeviceType
                    {
                        Id = apiItem.Id,
                    };
                    _wialonDeviceTypeRepository.Insert(localItem);
                    itemIsNew = true;
                    addedCount++;
                }

                var tcpPort = !string.IsNullOrEmpty(apiItem.TcpPort) && apiItem.TcpPort.All(char.IsDigit) && apiItem.TcpPort != "0" ? int.Parse(apiItem.TcpPort) : (int?)null;
                var udpPort = !string.IsNullOrEmpty(apiItem.UdpPort) && apiItem.UdpPort.All(char.IsDigit) && apiItem.UdpPort != "0" ? int.Parse(apiItem.UdpPort) : (int?)null;
                var serverAddress = (tcpPort ?? udpPort).HasValue ? apiDeviceTypes.HardwareGatewayDomain + ":" + (tcpPort ?? udpPort) : null;

                if (localItem.DeviceCategory != apiItem.DeviceCategory)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, DeviceCategory changed from {localItem.DeviceCategory} to {apiItem.DeviceCategory}");
                    }
                    localItem.DeviceCategory = apiItem.DeviceCategory;
                }
                if (localItem.Name != apiItem.Name)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, Name changed from {localItem.Name} to {apiItem.Name}");
                    }
                    localItem.Name = apiItem.Name;
                }
                if (localItem.TcpPort != tcpPort)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, TcpPort changed from {localItem.TcpPort} to {tcpPort}");
                    }
                    localItem.TcpPort = tcpPort;
                }
                if (localItem.UdpPort != udpPort)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, UdpPort changed from {localItem.UdpPort} to {udpPort}");
                    }
                    localItem.UdpPort = udpPort;
                }
                if (localItem.ServerAddress != serverAddress)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, ServerAddress changed from {localItem.ServerAddress} to {serverAddress}");
                    }
                    localItem.ServerAddress = serverAddress;
                }
                if (itemWasUpdated)
                {
                    updatedCount++;
                }
                else if (itemIsNew)
                {
                    Logger.Info($"Added wialon device type {localItem.Id}, {localItem.Name}, Server Address: {localItem.ServerAddress}");
                }

                i++;
                if (i > 300)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    i = 0;
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            Logger.Info($"Added {addedCount} wialon device types, updated {updatedCount} existing records, skipped {(apiDeviceTypes.Items.Count - addedCount - updatedCount)} unchanged records");
        }

        public async Task<SyncWithWialonResult> SyncWithWialon(SyncWithWialonInput input)
        {
            return await SyncWithWialonForCurrentTenantAsync(input);
        }

        private async Task<SyncWithWialonResult> SyncWithWialonForCurrentTenantAsync(SyncWithWialonInput input)
        {
            Logger.Info("Started sync with wialon");
            var result = new SyncWithWialonResult();
            var truckCurrentData = new List<TruckCurrentData>();

            if (!await IsDtdTrackerConfigured())
            {
                Logger.Info("DtdTracker is not configured");
                return result;
            }

            var localTrucks = await _truckRepository.GetAll()
                .Where(x => x.LocationId != null)
                .Select(x => new
                {
                    x.Id,
                    x.TruckCode,
                    x.DtdTrackerDeviceTypeId,
                    x.DtdTrackerDeviceTypeName,
                    x.DtdTrackerPassword,
                    x.DtdTrackerUniqueId,
                    x.IsActive,
                    x.VehicleCategory.IsPowered
                }).ToListAsync();

            Logger.Info($"{localTrucks.Count} local trucks");

            var loginResult = await _dtdTrackerTelematics.LoginToApi();

            var apiTrucks = await _dtdTrackerTelematics.GetAllUnits(loginResult);
            Logger.Info($"{apiTrucks.Count} remote trucks");

            foreach (var localTruck in localTrucks)
            {
                if (localTruck.DtdTrackerUniqueId.IsNullOrEmpty())
                {
                    Logger.Info($"Skipped local truck {localTruck.TruckCode} with missing unique id");
                    continue;
                }

                if (localTruck.IsActive)
                {
                    if (localTruck.DtdTrackerDeviceTypeId == null)
                    {
                        Logger.Info($"Skipped active local truck {localTruck.TruckCode} with missing device type id");
                        continue;
                    }

                    if (apiTrucks.Any(x => x.UniqueId == localTruck.DtdTrackerUniqueId))
                    {
                        Logger.Info($"Skipped local truck {localTruck.TruckCode} because a Unit with unique id {localTruck.DtdTrackerUniqueId} already exsits");
                        continue;
                    }

                    var unit = new UnitDto
                    {
                        Name = localTruck.TruckCode,
                        DeviceTypeId = localTruck.DtdTrackerDeviceTypeId.Value,
                        UniqueId = localTruck.DtdTrackerUniqueId,
                        Password = localTruck.DtdTrackerPassword
                    };
                    await _dtdTrackerTelematics.CreateUnit(unit, loginResult);
                    apiTrucks.Add(unit);
                }
                else
                {
                    var apiTruck = apiTrucks.FirstOrDefault(x => x.UniqueId == localTruck.DtdTrackerUniqueId);
                    if (apiTruck != null)
                    {
                        await _dtdTrackerTelematics.DeleteItem(apiTruck.Id);
                        Logger.Info($"Deleted item {apiTruck.Id} because Truck {localTruck.TruckCode} with unique id {localTruck.DtdTrackerUniqueId} is inactive");
                        apiTrucks.Remove(apiTruck);
                    }
                }
            }

            await _dtdTrackerTelematics.LogoutFromApi(loginResult);

            var apiTrucksToAddLocally = new List<Truck>();

            foreach (var apiTruck in apiTrucks)
            {
                if (!apiTruck.UniqueId.IsNullOrEmpty() && !localTrucks.Any(l => l.DtdTrackerUniqueId == apiTruck.UniqueId))
                {
                    apiTrucksToAddLocally.Add(new Truck
                    {
                        TruckCode = apiTruck.Name,
                        //VehicleCategoryId = vehicleCategory.Id,
                        CurrentHours = apiTruck.EngineHours,
                        CurrentMileage = UnitConverter.GetMiles(apiTruck.Mileage, apiTruck.MeasureUnits),
                        DtdTrackerDeviceTypeId = apiTruck.DeviceTypeId,
                        //DtdTrackerDeviceTypeName
                        DtdTrackerPassword = apiTruck.Password,
                        DtdTrackerUniqueId = apiTruck.UniqueId,
                        LocationId = OfficeId,
                        IsActive = true
                    });
                }
            }

            if (apiTrucksToAddLocally.Any())
            {
                var maxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
                var originalMaxNumberOfTrucks = maxNumberOfTrucks;
                int currentNumberOfTrucks = localTrucks.Count(t => t.IsPowered);

                var vehicleCategory = _vehicleCategoryRepository.GetAll().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).FirstOrDefault();
                if (vehicleCategory == null)
                {
                    throw new UserFriendlyException("No vehicle category was found");
                }

                var deviceTypeIds = apiTrucksToAddLocally.Select(x => x.DtdTrackerDeviceTypeId).Distinct().ToList();
                var deviceTypes = await _wialonDeviceTypeRepository.GetAll().Where(x => deviceTypeIds.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.ServerAddress }).ToListAsync();

                foreach (var truck in apiTrucksToAddLocally.ToList())
                {
                    if (currentNumberOfTrucks >= maxNumberOfTrucks)
                    {
                        if (input.IncreaseNumberOfTrucksIfNeeded)
                        {
                            await CurrentUnitOfWork.SaveChangesAsync();
                            maxNumberOfTrucks += apiTrucksToAddLocally.Count;
                            Logger.Info($"Going to increase number of trucks for tenant {Session.TenantId} from {originalMaxNumberOfTrucks} to {maxNumberOfTrucks}");
                            await _truckAppService.UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(new UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput
                            {
                                NewValue = maxNumberOfTrucks
                            });
                        }
                        else
                        {
                            result.AdditionalNumberOfTrucksRequired = apiTrucksToAddLocally.Count;
                            Logger.Info($"Not enough NumberOfTrucksFeature, needed {result.AdditionalNumberOfTrucksRequired} more");
                            break;
                        }
                    }

                    truck.VehicleCategoryId = vehicleCategory.Id;
                    var deviceType = deviceTypes.FirstOrDefault(x => x.Id == truck.DtdTrackerDeviceTypeId);
                    if (deviceType == null)
                    {
                        Logger.Error($"Wialon Device Type {truck.DtdTrackerDeviceTypeId} wasn't found in the local db for truck {truck.DtdTrackerUniqueId}");
                    }
                    else
                    {
                        truck.DtdTrackerDeviceTypeName = deviceType.Name;
                        truck.DtdTrackerServerAddress = deviceType.ServerAddress;
                    }

                    _truckRepository.Insert(truck);
                    Logger.Info($"Added local truck {truck.TruckCode} ({truck.DtdTrackerUniqueId})");
                    apiTrucksToAddLocally.Remove(truck);
                    currentNumberOfTrucks++;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task SyncWithIntelliShift()
        {
            await SyncWithIntelliShiftForCurrentTenantAsync();
        }

        private async Task SyncWithIntelliShiftForCurrentTenantAsync()
        {
            Logger.Info("Started sync with IntelliShift");
            var truckCurrentData = new List<TruckCurrentData>();

            if (!await IsIntelliShiftConfigured())
            {
                Logger.Info("IntelliShift is not configured");
                return;
            }

            var tokenLoginResult = await _intelliShiftTelematics.LoginToApiAsync();

            var localTrucks = await _truckRepository
                                .GetAll()
                                .Select(truck => truck)
                                .ToListAsync();

            Logger.Info($"{localTrucks.Count} local trucks");

            var apiTrucks = await _intelliShiftTelematics.GetAllUnitsAsync(tokenLoginResult);

            Logger.Info($"{apiTrucks.Count} remote trucks");

            foreach (var localTruck in localTrucks)
            {
                if (localTruck.TruckCode.IsNullOrEmpty())
                {
                    Logger.Info($"Skipped local truck {localTruck.TruckCode} with missing unique id");
                    continue;
                }

                TruckUnitDto apiTruck = apiTrucks.FirstOrDefault(p => p.Name == localTruck.TruckCode);
                if (apiTruck == null)
                {
                    Logger.Info($"Local truck {localTruck.TruckCode} is not registered in IntelliShift");
                    continue;
                }

                if (localTruck.IsActive)
                {
                    localTruck.CurrentHours = apiTruck.CumulativeHours ?? 0;
                    localTruck.CurrentMileage = apiTruck.Odometer ?? 0;
                    _truckRepository.Update(localTruck);
                }

                if (!localTruck.IsActive && apiTruck.IsActive)
                {
                    await _intelliShiftTelematics.UpdateUnit(apiTruck.Id,
                            tokenLoginResult, (nameof(localTruck.IsActive), localTruck.IsActive));
                }
            }

            var vehicleCategory = _vehicleCategoryRepository.GetAll()
                                                            .OrderBy(x => x.SortOrder)
                                                            .ThenBy(x => x.Id)
                                                            .FirstOrDefault();
            if (vehicleCategory == null)
            {
                throw new UserFriendlyException("No vehicle category was found");
            }

            var localTruckCodes = localTrucks.Select(p => p.TruckCode).ToList();
            var apiTrucksToAddLocally = apiTrucks
                                            .Where(apiTruck => !string.IsNullOrEmpty(apiTruck.Name) &&
                                                                !localTruckCodes.Contains(apiTruck.Name) &&
                                                                apiTruck.IsActive)
                                            .Select(apiTruck => apiTruck.ParseToTruck());

            apiTrucksToAddLocally.ForEach(truck =>
            {
                truck.LocationId = OfficeId;
                truck.VehicleCategoryId = vehicleCategory.Id;
                _truckRepository.Insert(truck);
                Logger.Info($"Added local truck {truck.TruckCode} ({truck.Plate})");
            });

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize]
        public async Task<bool> IsGpsIntegrationConfigured()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            switch (gpsPlatform)
            {
                case GpsPlatform.DtdTracker:
                    return !(await SettingManager.GetDtdTrackerSettingsAsync()).IsEmpty();
                case GpsPlatform.Geotab:
                    return !(await SettingManager.GetGeotabSettingsAsync()).IsEmpty();
                case GpsPlatform.Samsara:
                    return !(await SettingManager.GetSamsaraSettingsAsync()).IsEmpty();
                default:
                    return false;
            }
        }

        [AbpAuthorize]
        public async Task<bool> IsDtdTrackerConfigured()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            switch (gpsPlatform)
            {
                case GpsPlatform.DtdTracker:
                    return !(await SettingManager.GetDtdTrackerSettingsAsync()).IsEmpty();
                default:
                    return false;
            }
        }

        [AbpAuthorize]
        public async Task<bool> IsIntelliShiftConfigured()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            switch (gpsPlatform)
            {
                case GpsPlatform.IntelliShift:
                    return !(await SettingManager.GetIntelliShiftSettingsAsync()).IsEmpty();
                default:
                    return false;
            }
        }

        [RemoteService(false)]
        [UnitOfWork]
        public async Task UpdateMileageForAllTenantsAsync()
        {
            var tenants = _tenantRepository.GetAll()
                .Where(t => t.IsActive)
                .ToList();
            foreach (var tenant in tenants)
            {
                if (!await FeatureChecker.IsEnabledAsync(tenant.Id, AppFeatures.GpsIntegrationFeature))
                {
                    continue;
                }
                var platform = (GpsPlatform)await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.GpsIntegration.Platform, tenant.Id);

                using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                using (AbpSession.Use(tenant.Id, null))
                {
                    if (platform == GpsPlatform.Geotab)
                    {
                        var geotabSettings = await SettingManager.GetGeotabSettingsAsync();
                        if (geotabSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no geotab settings for TenantId={tenant.Id}");
                            continue;
                        }
                    }

                    if (platform == GpsPlatform.Samsara)
                    {
                        var samsaraSettings = await SettingManager.GetSamsaraSettingsAsync();
                        if (samsaraSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no samsara settings for TenantId={tenant.Id}");
                            continue;
                        }
                    }

                    if (platform == GpsPlatform.DtdTracker)
                    {
                        var dtdTrackerSettings = await SettingManager.GetDtdTrackerSettingsAsync();
                        if (dtdTrackerSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no DTD Tracker settings for TenantId={tenant.Id}");
                            continue;
                        }
                    }

                    if (platform == GpsPlatform.IntelliShift)
                    {
                        var intelliShiftSettings = await SettingManager.GetIntelliShiftSettingsAsync();
                        if (intelliShiftSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no Intellishift settings for TenantId={tenant.Id}");
                            continue;
                        }
                    }

                    Logger.Info($"Update mileage for TenantId={tenant.Id}");
                    await UpdateMileageForCurrentTenantAsync(true);
                }
            }
        }

        public async Task<string> TestUploadTruckPositionsToWialon()
        {
            await UploadTruckPositionsToWialonAsync();
            return "Done";
        }

        [UnitOfWork(false)]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        public void UploadTruckPositionsToWialon()
        {
            AsyncHelper.RunSync(() => UploadTruckPositionsToWialonAsync());
        }

        [UnitOfWork(false)]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        public async Task UploadTruckPositionsToWialonAsync()
        {
            var runId = Guid.NewGuid().ToShortGuid();
            try
            {
                Logger.Info($"UploadTruckPositionsToWialon|{runId}| started at {Clock.Now:s}");

                Infrastructure.Telematics.Dto.DtdTracker.TokenLoginResult loginResult = null;

                using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions
                {
                    IsTransactional = false,
                    Timeout = TimeSpan.FromMinutes(60),
                }))
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var lastUploadedId = await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionId);
                    var truckPositionsToUpload = _truckPositionRepository.GetAll()
                        .Where(x => x.Id > lastUploadedId && !string.IsNullOrEmpty(x.Truck.DtdTrackerUniqueId))
                        .Select(x => new
                        {
                            x.Id,
                            x.Truck.DtdTrackerUniqueId,
                            x.TruckId,
                            x.Timestamp,
                            x.Altitude,
                            x.Latitude,
                            x.Longitude,
                            x.Speed,
                        })
                        .ToList();

                    if (!truckPositionsToUpload.Any())
                    {
                        Logger.Warn($"UploadTruckPositionsToWialon|{runId}| Nothing to upload after id {lastUploadedId}");
                        unitOfWork.Complete();
                        return;
                    }
                    else
                    {
                        Logger.Warn($"UploadTruckPositionsToWialon|{runId}| Found {truckPositionsToUpload.Count} records to process after id {lastUploadedId}");
                    }

                    try
                    {
                        foreach (var group in truckPositionsToUpload.GroupBy(x => x.DtdTrackerUniqueId))
                        {
                            var dtdTrackerUniqueId = group.Key;
                            if (dtdTrackerUniqueId.IsNullOrEmpty())
                            {
                                continue;
                            }

                            if (loginResult == null)
                            {
                                loginResult = await _dtdTrackerTelematics.LoginToApi();
                            }

                            var unit = await _dtdTrackerTelematics.GetUnitByUniqueId(dtdTrackerUniqueId, loginResult);
                            if (unit == null)
                            {
                                Logger.Warn($"UploadTruckPositionsToWialon|{runId}| Unit with unique id {dtdTrackerUniqueId} wasn't found");
                                continue;
                            }

                            var messages = group.Where(x => x.Latitude.HasValue && x.Longitude.HasValue).Select(x => new GpsMessageDto
                            {
                                Timestamp = x.Timestamp,
                                AltitudeInMeters = x.Altitude,
                                Latitude = x.Latitude.Value,
                                Longitude = x.Longitude.Value,
                                SpeedInKMPH = (int)Math.Round((x.Speed ?? 0) * 3.6M, 0) //m/s to km/h
                            }).ToList();

                            Logger.Info($"UploadTruckPositionsToWialon|{runId}| Uploading {messages.Count} messages for truck with unique id {dtdTrackerUniqueId} (unitId {unit.Id})");
                            await _dtdTrackerTelematics.ImportMessages(unit.Id, messages, loginResult);
                            Logger.Info($"UploadTruckPositionsToWialon|{runId}| Finished uploading for truck {dtdTrackerUniqueId} (unitId {unit.Id})");

                        }

                        var newLastId = truckPositionsToUpload.Max(x => x.Id);
                        var currentLastId = await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionId);
                        if (currentLastId != lastUploadedId && currentLastId >= newLastId)
                        {
                            Logger.Warn($"UploadTruckPositionsToWialon|{runId}| Didn't update LastUploadedTruckPositionId to {newLastId} because the current value is already higher or equal ({currentLastId})");
                        }
                        else
                        {
                            await SettingManager.ChangeSettingForApplicationAsync(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionId, newLastId.ToString());
                            Logger.Info($"UploadTruckPositionsToWialon|{runId}| Changed LastUploadedTruckPositionId to {newLastId}");
                        }

                        unitOfWork.Complete();
                    }
                    finally
                    {
                        if (loginResult != null)
                        {
                            await _dtdTrackerTelematics.LogoutFromApi(loginResult);
                        }
                    }
                }
                Logger.Info($"UploadTruckPositionsToWialon|{runId}| finished at {Clock.Now:s}");
            }
            catch (Exception e)
            {
                Logger.Error($"UploadTruckPositionsToWialon|{runId}| failed", e);
                throw;
            }
        }
    }
}
