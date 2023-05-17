﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.Trucks.Exporting;
using DispatcherWeb.VehicleCategories;
using DispatcherWeb.VehicleMaintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Trucks
{
    [AbpAuthorize]
    public class TruckAppService : DispatcherWebAppServiceBase, ITruckAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<TruckFile> _truckFileRepository;
        private readonly IRepository<SharedTruck> _sharedTruckRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<OutOfServiceHistory> _outOfServiceHistoryRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly ITruckListCsvExporter _truckListCsvExporter;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IEmailSender _emailSender;
        private readonly IAppNotifier _appNotifier;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;

        public TruckAppService(
            IRepository<Truck> truckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<TruckFile> truckFileRepository,
            IRepository<SharedTruck> sharedTruckRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Office> officeRepository,
            IRepository<OutOfServiceHistory> outOfServiceHistoryRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<TimeOff> timeOffRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            ISingleOfficeAppService singleOfficeService,
            ITruckListCsvExporter truckListCsvExporter,
            IDriverApplicationPushSender driverApplicationPushSender,
            ISyncRequestSender syncRequestSender,
            IEmailSender emailSender,
            IAppNotifier appNotifier,
            ICrossTenantOrderSender crossTenantOrderSender
            )
        {
            _truckRepository = truckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _truckFileRepository = truckFileRepository;
            _sharedTruckRepository = sharedTruckRepository;
            _orderRepository = orderRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _officeRepository = officeRepository;
            _outOfServiceHistoryRepository = outOfServiceHistoryRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _dispatchRepository = dispatchRepository;
            _ticketRepository = ticketRepository;
            _timeOffRepository = timeOffRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _singleOfficeService = singleOfficeService;
            _truckListCsvExporter = truckListCsvExporter;
            _driverApplicationPushSender = driverApplicationPushSender;
            _syncRequestSender = syncRequestSender;
            _emailSender = emailSender;
            _appNotifier = appNotifier;
            _crossTenantOrderSender = crossTenantOrderSender;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<PagedResultDto<TruckDto>> GetTrucks(GetTrucksInput input)
        {
            DateTime today = await GetToday();
            var query = GetFilteredTruckQuery(input, today);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new TruckDto
                {
                    Id = x.Id,
                    TruckCode = x.TruckCode,
                    OfficeName = x.Office.Name,
                    VehicleCategoryName = x.VehicleCategory.Name,
                    DefaultDriverName = x.DefaultDriver != null ? x.DefaultDriver.LastName + ", " + x.DefaultDriver.FirstName : "",
                    IsActive = x.IsActive,
                    IsOutOfService = x.IsOutOfService,
                    CurrentMileage = x.CurrentMileage,
                    DueDateStatus = x.PreventiveMaintenances
                            .OrderBy(pm => pm.DueDate)
                            .Select(pm => pm.DueDate < today ? true : (pm.WarningDate < today ? false : (bool?)null))
                            .FirstOrDefault(),
                    DueMileageStatus = x.PreventiveMaintenances
                            .OrderBy(pm => pm.DueMileage)
                            .Select(pm => pm.DueMileage < x.CurrentMileage ? true : (pm.WarningMileage < x.CurrentMileage ? false : (bool?)null))
                            .FirstOrDefault(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<TruckDto>(
                totalCount,
                items);
        }

        private IQueryable<Truck> GetFilteredTruckQuery(IGetTruckListFilter input, DateTime today) =>
            _truckRepository.GetAll()
                .Where(x => x.LocationId.HasValue && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true)
                .WhereIf(input.OfficeId.HasValue, x => x.LocationId == input.OfficeId)
                .WhereIf(input.VehicleCategoryId > 0, x => x.VehicleCategoryId == input.VehicleCategoryId)
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .WhereIf(input.IsOutOfService.HasValue, x => x.IsOutOfService == input.IsOutOfService)
                .WhereIf(input.PlatesExpiringThisMonth, t =>
                    t.PlateExpiration.HasValue &&
                    t.PlateExpiration.Value.Year == today.Year &&
                    t.PlateExpiration.Value.Month == today.Month
                );

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        [HttpPost]
        public async Task<FileDto> GetTrucksToCsv(GetTrucksInput input)
        {
            DateTime today = await GetToday();
            var query = GetFilteredTruckQuery(input, today);
            var items = await GetTruckEditDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return _truckListCsvExporter.ExportToFile(items);

        }

        private IQueryable<TruckEditDto> GetTruckEditDtoQuery(IQueryable<Truck> query) =>
            query.Select(x => new TruckEditDto()
            {
                Id = x.Id,
                TruckCode = x.TruckCode,
                OfficeName = x.Office.Name,
                VehicleCategoryId = x.VehicleCategoryId,
                VehicleCategoryName = x.VehicleCategory.Name,
                DefaultDriverName = x.DefaultDriver != null ? x.DefaultDriver.LastName + ", " + x.DefaultDriver.FirstName : "",
                DefaultTrailerCode = x.DefaultTrailer.TruckCode,
                IsActive = x.IsActive,
                InactivationDate = x.InactivationDate,
                IsOutOfService = x.IsOutOfService,
                Reason = x.IsOutOfService ? x.OutOfServiceHistories.OrderByDescending(oosh => oosh.OutOfServiceDate).Select(oosh => oosh.Reason).FirstOrDefault() : "",
                Year = x.Year,
                Make = x.Make,
                Model = x.Model,
                InServiceDate = x.InServiceDate,
                Vin = x.Vin,
                Plate = x.Plate,
                PlateExpiration = x.PlateExpiration,
                CargoCapacity = x.CargoCapacity,
                CargoCapacityCyds = x.CargoCapacityCyds,
                InsurancePolicyNumber = x.InsurancePolicyNumber,
                InsuranceValidUntil = x.InsuranceValidUntil,
                PurchaseDate = x.PurchaseDate,
                PurchasePrice = x.PurchasePrice,
                SoldDate = x.SoldDate,
                SoldPrice = x.SoldPrice,
                TruxTruckId = x.TruxTruckId,
                BedConstruction = x.BedConstruction,
                CanPullTrailer = x.CanPullTrailer,
                CurrentHours = x.CurrentHours,
                CurrentMileage = x.CurrentMileage,
                DefaultDriverId = x.DefaultDriverId,
                DefaultTrailerId = x.DefaultTrailerId,
                DtdTrackerDeviceTypeId = x.DtdTrackerDeviceTypeId,
                DtdTrackerDeviceTypeName = x.DtdTrackerDeviceTypeName,
                DtdTrackerPassword = x.DtdTrackerPassword,
                DtdTrackerServerAddress = x.DtdTrackerServerAddress,
                DtdTrackerUniqueId = x.DtdTrackerUniqueId,
                IsApportioned = x.IsApportioned,
                OfficeId = x.LocationId == null ? 0 : x.LocationId.Value,
                VehicleCategoryAssetType = x.VehicleCategory.AssetType,
                VehicleCategoryIsPowered = x.VehicleCategory.IsPowered,
                FuelType = x.FuelType,
                FuelCapacity = x.FuelCapacity,
                SteerTires = x.SteerTires,
                DriveAxleTires = x.DriveAxleTires,
                DropAxleTires = x.DropAxleTires,
                TrailerTires = x.TrailerTires,
                Transmission = x.Transmission,
                Engine = x.Engine,
                RearEnd = x.RearEnd,
            });


        public async Task<PagedResultDto<SelectListDto>> GetTrucksSelectList(GetTrucksSelectListInput input)
        {
            var query = _truckRepository.GetAll();

            if (input.OrderLineId.HasValue)
            {
                query = _orderLineTruckRepository.GetAll()
                    .Where(x => x.OrderLineId == input.OrderLineId)
                    .Select(x => x.Truck)
                    .Distinct();
            }
            query = query
                //.Where(x => x.LocationId.HasValue)
                .WhereIf(!input.AllOffices && input.OfficeId == null,
                    x => x.LocationId == OfficeId
                        || input.IncludeLeaseHaulerTrucks && x.LocationId == null)
                .WhereIf(input.OfficeId != null,
                    x => x.LocationId == input.OfficeId
                        || input.IncludeLeaseHaulerTrucks && x.LocationId == null)
                .WhereIf(!input.IncludeLeaseHaulerTrucks, x => x.LocationId.HasValue)
                .WhereIf(input.InServiceOnly, x => !x.IsOutOfService)
                .WhereIf(input.ActiveOnly, x => x.IsActive)
                .WhereIf(input.ExcludeTrailers, x => x.VehicleCategory.IsPowered);

            return await query
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.TruckCode
                })
                .GetSelectListResult(input);
        }

        public async Task<PagedResultDto<SelectListDto>> GetVehicleCategoriesSelectList(GetVehicleCategoriesSelectListInput input)
        {
            var categoriesInUse = input.IsInUse == true ? await _truckRepository.GetAll().Select(x => x.VehicleCategoryId).Distinct().ToListAsync() : new List<int>();

            return await _vehicleCategoryRepository.GetAll()
                .WhereIf(input.IsPowered.HasValue, x => x.IsPowered == input.IsPowered)
                .WhereIf(input.IsInUse == true, x => categoriesInUse.Contains(x.Id))
                .Select(x => new SelectListDto<VehicleCategorySelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new VehicleCategorySelectListInfoDto
                    {
                        AssetType = x.AssetType,
                        IsPowered = x.IsPowered
                    }
                })
                .GetSelectListResult(input);
        }

        public async Task<List<VehicleCategoryDto>> GetVehicleCategories()
        {
            return await _vehicleCategoryRepository.GetAll()
                .Select(x => new VehicleCategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    AssetType = x.AssetType,
                    IsPowered = x.IsPowered,
                    SortOrder = x.SortOrder
                })
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        public async Task<PagedResultDto<SelectListDto>> GetActiveTrailersSelectList(GetSelectListInput input) =>
            await _truckRepository.GetAll()
                .Where(x => x.LocationId.HasValue)
                .Where(t => t.VehicleCategory.AssetType == AssetType.Trailer && t.IsActive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.TruckCode
                })
                .GetSelectListResult(input);

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<TruckEditDto> GetTruckForEdit(GetTruckForEditInput input)
        {
            TruckEditDto truckEditDto;

            if (input.Id.HasValue)
            {
                truckEditDto = await _truckRepository.GetAll()
                    .Select(t => new TruckEditDto
                    {
                        Id = t.Id,
                        TruckCode = t.TruckCode,
                        OfficeId = t.LocationId == null ? 0 : t.LocationId.Value,
                        OfficeName = t.Office.Name,
                        VehicleCategoryId = t.VehicleCategoryId,
                        VehicleCategoryName = t.VehicleCategory.Name,
                        VehicleCategoryAssetType = t.VehicleCategory.AssetType,
                        VehicleCategoryIsPowered = t.VehicleCategory.IsPowered,
                        BedConstruction = t.BedConstruction,
                        CanPullTrailer = t.CanPullTrailer,
                        IsApportioned = t.IsApportioned,
                        DefaultDriverId = t.DefaultDriverId,
                        DefaultDriverName = t.DefaultDriver != null ? t.DefaultDriver.FirstName + " " + t.DefaultDriver.LastName : "",
                        DefaultTrailerId = t.DefaultTrailerId,
                        DefaultTrailerCode = t.DefaultTrailer.TruckCode,
                        IsActive = t.IsActive,
                        InactivationDate = t.InactivationDate,
                        IsOutOfService = t.IsOutOfService,

                        CurrentMileage = t.CurrentMileage,
                        CurrentHours = t.CurrentHours,
                        Year = t.Year,
                        Make = t.Make,
                        Model = t.Model,
                        Vin = t.Vin,
                        Plate = t.Plate,
                        PlateExpiration = t.PlateExpiration,
                        CargoCapacity = t.CargoCapacity,
                        CargoCapacityCyds = t.CargoCapacityCyds,
                        FuelType = t.FuelType,
                        FuelCapacity = t.FuelCapacity,
                        SteerTires = t.SteerTires,
                        DriveAxleTires = t.DriveAxleTires,
                        DropAxleTires = t.DropAxleTires,
                        TrailerTires = t.TrailerTires,
                        Transmission = t.Transmission,
                        Engine = t.Engine,
                        RearEnd = t.RearEnd,
                        InsurancePolicyNumber = t.InsurancePolicyNumber,
                        InsuranceValidUntil = t.InsuranceValidUntil,
                        PurchaseDate = t.PurchaseDate,
                        PurchasePrice = t.PurchasePrice,
                        InServiceDate = t.InServiceDate,
                        SoldDate = t.SoldDate,
                        TruxTruckId = t.TruxTruckId,
                        SoldPrice = t.SoldPrice,
                        DtdTrackerUniqueId = t.DtdTrackerUniqueId,
                        DtdTrackerDeviceTypeId = t.DtdTrackerDeviceTypeId,
                        DtdTrackerDeviceTypeName = t.DtdTrackerDeviceTypeName,
                        DtdTrackerPassword = t.DtdTrackerPassword,
                        DtdTrackerServerAddress = t.DtdTrackerServerAddress,
                        Reason = t.IsOutOfService ?
                        t.OutOfServiceHistories
                            .OrderByDescending(oosh => oosh.OutOfServiceDate)
                            .Select(oosh => oosh.Reason).FirstOrDefault()
                        : "",

                        Files = t.Files.Select(f => new TruckFileEditDto()
                        {
                            Id = f.Id,
                            TruckId = f.TruckId,
                            Title = f.Title,
                            Description = f.Description,
                            FileId = f.FileId,
                            ThumbnailId = f.ThumbnailId,
                            FileName = f.FileName,
                            FileType = f.FileType,
                        }).ToList(),

                    })
                    .SingleAsync(t => t.Id == input.Id.Value);
            }
            else
            {
                truckEditDto = new TruckEditDto
                {
                    TruckCode = input.Name,
                    IsActive = true,
                    Files = new List<TruckFileEditDto>(),
                    InServiceDate = await GetToday()
                };

                if (input.VehicleCategoryId.HasValue)
                {
                    var vehicleCategory = await _vehicleCategoryRepository.GetAll()
                        .Where(x => x.Id == input.VehicleCategoryId)
                        .Select(x => new
                        {
                            x.Id,
                            x.Name,
                            x.AssetType,
                            x.IsPowered
                        }).FirstOrDefaultAsync();

                    if (vehicleCategory != null)
                    {
                        truckEditDto.VehicleCategoryId = vehicleCategory.Id;
                        truckEditDto.VehicleCategoryName = vehicleCategory.Name;
                        truckEditDto.VehicleCategoryAssetType = vehicleCategory.AssetType;
                        truckEditDto.VehicleCategoryIsPowered = vehicleCategory.IsPowered;
                    }
                }
            }

            await _singleOfficeService.FillSingleOffice(truckEditDto);

            return truckEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<EditTruckResult> EditTruck(TruckEditDto model)
        {
            EditTruckResult result = new EditTruckResult();

            Truck entity = model.Id.HasValue ? await _truckRepository.GetAsync(model.Id.Value) : new Truck();

            var newVehicleCategory = await _vehicleCategoryRepository.GetAll().Where(x => x.Id == model.VehicleCategoryId).FirstOrDefaultAsync();
            if (newVehicleCategory == null)
            {
                throw new UserFriendlyException("Category is required");
            }
            var oldVehicleCategory = entity.VehicleCategoryId == model.VehicleCategoryId
                ? newVehicleCategory
                : entity.VehicleCategoryId != 0
                    ? await _vehicleCategoryRepository.GetAll().Where(x => x.Id == entity.VehicleCategoryId).FirstOrDefaultAsync()
                    : null;

            if ((!model.Id.HasValue || newVehicleCategory != oldVehicleCategory) && newVehicleCategory.IsPowered)
            {
                var currentNumberOfTrucks = await _truckRepository.CountAsync(t => t.LocationId != null && t.VehicleCategory.IsPowered && t.Id != model.Id) + 1;
                var maxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
                if (currentNumberOfTrucks > maxNumberOfTrucks)
                {
                    result.NeededBiggerNumberOfTrucks = currentNumberOfTrucks;
                    return result;
                }
            }

            if (!model.IsActive && model.InactivationDate == null)
            {
                throw new ArgumentException("The InactivationDate must be specified when IsActive=false!");
            }

            if (newVehicleCategory.AssetType == AssetType.Trailer && model.DefaultDriverId.HasValue)
            {
                throw new ArgumentException("A Trailer cannot have a default driver!");
            }
            else if (!newVehicleCategory.IsPowered && model.DefaultDriverId.HasValue)
            {
                throw new ArgumentException("An unpowered vehicle cannot have a default driver!");
            }

            await CreateOrUpdateOutOfServiceHistory(entity, model.IsOutOfService, model.Reason);
            result.ThereWereAssociatedOrders = await RemoveTruckFromScheduleIfTruckIsOutOfService(entity.Id, model.IsOutOfService);
            await RemoveTruckFromScheduleAndDriverAssignmentIfTruckIsNotIsActive(entity.Id, model.IsActive, model.InactivationDate);
            result.ThereWereCanceledDispatches = await CancelUnacknowledgedDispatchesIfTruckIsOutOfServiceOrIsNotIsActive(entity.Id, model.IsOutOfService, model.IsActive);
            result.ThereWereNotCanceledDispatches = await ThereAreActiveOrLoadedDispatchesAndTruckIsOutOfServiceOrIsNotIsActive(entity.Id, model.IsOutOfService, model.IsActive);
            if (await UpdateDefaultDriver())
            {
                result.ThereAreOrdersInTheFuture = await ThereAreOrdersInTheFuture();
            }
            result.ThereWereAssociatedOrders |= await UpdateOffice();
            await ThrowUserFriendlyExceptionIfTruckCodeExists(model.TruckCode, entity.Id, model.OfficeId);
            if (model.Id.HasValue && newVehicleCategory.AssetType == AssetType.Trailer && model.IsActive != true)
            {
                var tractors = await _truckRepository.GetAll().Where(x => x.DefaultTrailerId == model.Id).ToListAsync();
                tractors.ForEach(x => x.DefaultTrailerId = null);
            }
            await ThrowUserFriendlyExceptionIfTruckWasTrailerAndCategoryChanged();

            entity.TruckCode = model.TruckCode;
            entity.VehicleCategoryId = model.VehicleCategoryId;
            entity.IsActive = model.IsActive;
            entity.InactivationDate = model.IsActive ? null : model.InactivationDate;
            entity.IsOutOfService = model.IsOutOfService;
            entity.IsApportioned = model.IsApportioned;
            entity.BedConstruction = model.BedConstruction;
            entity.CanPullTrailer = model.CanPullTrailer;

            entity.CurrentMileage = model.CurrentMileage;
            entity.CurrentHours = model.CurrentHours;
            entity.Year = model.Year;
            entity.Make = model.Make;
            entity.Model = model.Model;
            entity.Vin = model.Vin;
            entity.Plate = model.Plate;
            entity.PlateExpiration = model.PlateExpiration;
            entity.CargoCapacity = model.CargoCapacity;
            entity.CargoCapacityCyds = model.CargoCapacityCyds;
            entity.FuelType = model.FuelType;
            entity.FuelCapacity = model.FuelCapacity;
            entity.SteerTires = model.SteerTires;
            entity.DriveAxleTires = model.DriveAxleTires;
            entity.DropAxleTires = model.DropAxleTires;
            entity.TrailerTires = model.TrailerTires;
            entity.Transmission = model.Transmission;
            entity.Engine = model.Engine;
            entity.RearEnd = model.RearEnd;
            entity.InsurancePolicyNumber = model.InsurancePolicyNumber;
            entity.InsuranceValidUntil = model.InsuranceValidUntil;
            entity.PurchaseDate = model.PurchaseDate;
            entity.PurchasePrice = model.PurchasePrice;
            entity.InServiceDate = model.InServiceDate;
            entity.SoldDate = model.SoldDate;
            entity.SoldPrice = model.SoldPrice;
            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings))
            {
                entity.TruxTruckId = model.TruxTruckId;
            }
            if (await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                if (model.DtdTrackerUniqueId.IsNullOrEmpty())
                {
                    var tenantDetails = await TenantManager.Tenants
                        .Where(x => x.Id == Session.TenantId)
                        .Select(x => new
                        {
                            x.Name
                        }).FirstOrDefaultAsync();

                    model.DtdTrackerUniqueId = tenantDetails.Name + model.TruckCode;
                }
                entity.DtdTrackerUniqueId = model.DtdTrackerUniqueId;
                entity.DtdTrackerDeviceTypeId = model.DtdTrackerDeviceTypeId;
                entity.DtdTrackerDeviceTypeName = model.DtdTrackerDeviceTypeName;
                entity.DtdTrackerPassword = model.DtdTrackerPassword;
                entity.DtdTrackerServerAddress = model.DtdTrackerServerAddress;
            }

            await UpdateDefaultTrailer();

            if (model.Id.HasValue)
            {
                await UpdateTruckFiles(entity.Id, model.Files);
            }

            result.Id = await _truckRepository.InsertOrUpdateAndGetIdAsync(entity);

            await _crossTenantOrderSender.SyncMaterialCompanyTrucksIfNeeded(entity.Id);

            return result;

            // Local functions
            async Task<bool> UpdateDefaultDriver()
            {
                if (model.DefaultDriverId.HasValue)
                {
                    var otherTrucksWithSameDefaultDriver = _truckRepository.GetAll()
                        .Where(t =>
                            t.LocationId != null
                            && t.Id != entity.Id && t.DefaultDriverId == model.DefaultDriverId)
                        .ToList();
                    otherTrucksWithSameDefaultDriver.ForEach(x => x.DefaultDriverId = null);
                }

                if (entity.DefaultDriverId == model.DefaultDriverId)
                {
                    return false;
                }

                await SetDriverIdNullInDriverAssignments(entity.Id, entity.DefaultDriverId);

                entity.DefaultDriverId = model.DefaultDriverId;

                if (entity.Id == 0 && model.DefaultDriverId.HasValue)
                {
                    //we need to create driver assignments for the existing default driver with time off records, and for that we need to have truckId
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                await AddTimeOffDriverAssignmentsIfNeeded(entity.Id, model.DefaultDriverId);

                return true;
            }
            async Task<bool> ThereAreOrdersInTheFuture()
            {
                DateTime today = await GetToday();
                return await _orderLineTruckRepository.GetAll()
                    .AnyAsync(olt => olt.TruckId == entity.Id && olt.OrderLine.Order.DeliveryDate > today);
            }

            async Task<bool> UpdateOffice()
            {
                if (!OfficeIsChanged())
                {
                    return false;
                }
                entity.LocationId = model.OfficeId;
                if (model.Id.HasValue)
                {
                    await RemoveTruckFromDriverAssignmentStartingOnDate(model.Id.Value, null);
                    return await RemoveTruckFromScheduleStartingOnDate(model.Id.Value);
                }
                return false;

                // Local functions
                bool OfficeIsChanged() => entity.LocationId != model.OfficeId;
            }

            async Task UpdateDefaultTrailer()
            {
                if (!entity.CanPullTrailer && model.DefaultTrailerId != null)
                {
                    throw new ArgumentException("The truck must be able to pull a trailer to set a DefaultTrailerId");
                }
                if (entity.CanPullTrailer && model.DefaultTrailerId != null)
                {
                    var trailer = await _truckRepository.GetAll().Where(t => t.Id == model.DefaultTrailerId).Select(t => new { t.VehicleCategory.AssetType, t.IsActive }).FirstOrDefaultAsync();
                    if (trailer.AssetType != AssetType.Trailer)
                    {
                        throw new UserFriendlyException("The default trailer must be a trailer!");
                    }
                    if (!trailer.IsActive)
                    {
                        throw new UserFriendlyException("The default trailer must be active!");
                    }
                    var tractorWithDefaultTrailer = await _truckRepository.GetAll()
                        .WhereIf(model.Id != null, t => t.Id != model.Id)
                        .Where(t => t.DefaultTrailerId == model.DefaultTrailerId)
                        .FirstOrDefaultAsync();
                    if (tractorWithDefaultTrailer != null)
                    {
                        tractorWithDefaultTrailer.DefaultTrailerId = null;
                    }
                }

                entity.DefaultTrailerId = model.DefaultTrailerId;
            }
            async Task<bool> CancelUnacknowledgedDispatchesIfTruckIsOutOfServiceOrIsNotIsActive(int truckId, bool isOutOfService, bool isActive)
            {
                if (truckId == 0 || !isOutOfService && isActive)
                {
                    return false;
                }
                return await CancelUnacknowledgedDispatches(truckId);
            }
            async Task<bool> ThereAreActiveOrLoadedDispatchesAndTruckIsOutOfServiceOrIsNotIsActive(int truckId, bool isOutOfService, bool isActive)
            {
                if (truckId == 0 || !isOutOfService && isActive)
                {
                    return false;
                }
                return await ThereAreAcknowledgedOrLoadedDispatches(truckId);
            }

            async Task ThrowUserFriendlyExceptionIfTruckCodeExists(string truckCode, int currentTruckId, int officeId)
            {
                if (await _truckRepository.GetAll()
                        .AnyAsync(t => t.Id != currentTruckId && t.TruckCode == truckCode
                            && t.LocationId == officeId && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true))
                {
                    throw new UserFriendlyException($"A truck with truck code '{truckCode}' already exists in the same office.");
                }
            }

            async Task ThrowUserFriendlyExceptionIfTruckWasTrailerAndCategoryChanged()
            {
                if (model.Id != null
                    && oldVehicleCategory.AssetType == AssetType.Trailer
                    && newVehicleCategory.AssetType != AssetType.Trailer)
                {
                    if (model.IsActive)
                    {
                        var tractorCode = await GetTractorWithDefaultTrailer(new TrailerIsSetAsDefaultTrailerForAnotherTractorInput { TrailerId = model.Id.Value });
                        if (!string.IsNullOrEmpty(tractorCode))
                        {
                            throw new UserFriendlyException($"This trailer is currently the default trailer on {tractorCode} and can't be changed while assigned to a truck.");
                        }
                    }
                    if (await TruckHasHistory(model.Id.Value))
                    {
                        throw new UserFriendlyException($"This trailer already has history, so its category can't be changed.");
                    }
                }
                if (model.Id != null
                    && oldVehicleCategory.AssetType != AssetType.Trailer
                    && newVehicleCategory.AssetType == AssetType.Trailer)
                {
                    if (await TruckHasHistory(model.Id.Value))
                    {
                        throw new UserFriendlyException($"This truck already has history, so its category can't be changed.");
                    }
                }
            }
        }

        private async Task SetDriverIdNullInDriverAssignments(int truckId, int? driverId)
        {
            if (truckId == 0 || !driverId.HasValue)
            {
                return;
            }
            var today = await GetToday();
            var driverAssignments = await _driverAssignmentRepository.GetAll()
                .Where(da => da.DriverId == driverId && da.TruckId == truckId && da.Date >= today)
                .ToListAsync();
            foreach (var driverAssignment in driverAssignments)
            {
                driverAssignment.DriverId = null;
            }
        }

        private async Task AddTimeOffDriverAssignmentsIfNeeded(int truckId, int? driverId)
        {
            if (truckId == 0 || !driverId.HasValue)
            {
                return;
            }
            var today = await GetToday();

            var timeOffs = _timeOffRepository.GetAll()
                .Where(x => x.DriverId == driverId && x.EndDate >= today)
                .Select(x => new { x.StartDate, x.EndDate })
                .ToList();

            if (!timeOffs.Any())
            {
                return;
            }

            var allShifts = await SettingManager.GetShiftsAsync();
            var existingAssignments = await _driverAssignmentRepository.GetAll()
                .Where(da => da.TruckId == truckId && da.Date >= today)
                .ToListAsync();
            var sharedTruckResult = await _truckRepository.EnsureCanEditTruckOrSharedTruckAsync(truckId, OfficeId, timeOffs.Min(x => x.StartDate), timeOffs.Max(x => x.EndDate));

            foreach (var timeOff in timeOffs)
            {
                var date = timeOff.StartDate < today ? today : timeOff.StartDate;
                if (date < today)
                {
                    date = today;
                }

                while (date <= timeOff.EndDate)
                {
                    foreach (var shift in allShifts)
                    {
                        var existingAssignment = existingAssignments.FirstOrDefault(x => x.Date == date && x.Shift == shift);
                        if (existingAssignment == null)
                        {
                            var newDriverAssignment = new DriverAssignment
                            {
                                OfficeId = sharedTruckResult.GetLocationForDate(date, shift),
                                Date = date,
                                Shift = shift,
                                TruckId = truckId,
                                DriverId = null
                            };
                            await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                            existingAssignments.Add(newDriverAssignment);
                        }
                    }
                    date = date.AddDays(1);
                }
            }
        }

        private async Task<bool> TruckHasHistory(int truckId)
        {
            return await _ticketRepository.GetAll().AnyAsync(x => x.TruckId == truckId)
                || await _orderLineTruckRepository.GetAll().AnyAsync(x => x.TruckId == truckId);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<string> GetTractorWithDefaultTrailer(TrailerIsSetAsDefaultTrailerForAnotherTractorInput input) =>
            await _truckRepository.GetAll()
                .WhereIf(input.TractorId != null, t => t.Id != input.TractorId)
                .Where(t => t.DefaultTrailerId == input.TrailerId)
                .Select(t => t.TruckCode)
                .FirstOrDefaultAsync();

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<string> GetTruckCodeByDefaultDriver(GetTruckCodeByDefaultDriverInput input) =>
            await _truckRepository.GetAll()
                .WhereIf(input.ExceptTruckId.HasValue, t => t.Id != input.ExceptTruckId)
                .Where(t => t.LocationId != null
                    && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true
                    && t.DefaultDriverId == input.DefaultDriverId)
                .Select(t => t.TruckCode)
                .FirstOrDefaultAsync();

        [AbpAuthorize(AppPermissions.Pages_Trucks, AppPermissions.Pages_Schedule)]
        public async Task<string> GetTruckCodeByTruckId(int truckId)
        {
            return await _truckRepository.GetAll()
                .Where(t => t.Id == truckId)
                .Select(t => t.TruckCode)
                .FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task RemoveDefaultDriver(int truckId, int driverId)
        {
            DateTime today = await GetToday();
            await RemoveTodayAndFutureDriverAssignmentsForTruckAndDriver(truckId, driverId, today);
            await CurrentUnitOfWork.SaveChangesAsync();
            await CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(truckId, driverId, today);

        }
        [RemoteService(IsEnabled = false)]
        public async Task RemoveTodayAndFutureDriverAssignmentsForTruckAndDriver(int truckId, int driverId, DateTime today) =>
            await _driverAssignmentRepository.DeleteAsync(da => da.TruckId == truckId && da.DriverId == driverId && da.Date >= today);

        [RemoteService(IsEnabled = false)]
        public async Task CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(int truckId, int driverId, DateTime today)
        {
            var orderLineTruckQuery = _orderLineTruckRepository.GetAll()
                .Where(olt => olt.TruckId == truckId && olt.DriverId == driverId && olt.OrderLine.Order.DeliveryDate >= today);
            var driverAssignmentQuery = _driverAssignmentRepository.GetAll()
                .Where(da => da.TruckId == truckId && da.Date >= today && da.DriverId != null);
            var orderLineTrucksToDeleteOrCompleteQuery =
                from olt in orderLineTruckQuery
                join da in driverAssignmentQuery on new { olt.TruckId, Date = olt.OrderLine.Order.DeliveryDate.Value } equals new { da.TruckId, da.Date }
                    into gj
                from driverAssignment in gj.DefaultIfEmpty()
                where driverAssignment == null
                select new { olt.Id, olt.OrderLine.Order.DeliveryDate };

            var idsToDeleteOrComplete = await orderLineTrucksToDeleteOrCompleteQuery.ToListAsync();
            var idGroupsToDeleteOrComplete = idsToDeleteOrComplete.GroupBy(x => new { MarkComplete = x.DeliveryDate == today });
            foreach (var idGroup in idGroupsToDeleteOrComplete)
            {
                var ids = idGroup.Select(x => x.Id).ToList();
                var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                    .Where(x => ids.Contains(x.Id) || x.ParentOrderLineTruckId.HasValue && ids.Contains(x.ParentOrderLineTruckId.Value))
                    .ToListAsync();
                if (idGroup.Key.MarkComplete)
                {
                    orderLineTrucks.ForEach(olt =>
                    {
                        olt.IsDone = true;
                        olt.Utilization = 0;
                    });
                }
                else
                {
                    orderLineTrucks.ForEach(_orderLineTruckRepository.Delete);
                    var orderLineIds = orderLineTrucks.Select(x => x.OrderLineId).Distinct().ToList();
                    await CurrentUnitOfWork.SaveChangesAsync();
                    foreach (var orderLineId in orderLineIds)
                    {
                        var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                        var order = await orderLineUpdater.GetOrderAsync();
                        if (order.DeliveryDate >= await GetToday())
                        {
                            orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                            await orderLineUpdater.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        private async Task CreateOrUpdateOutOfServiceHistory(Truck entity, bool isOutOfService, string reason)
        {
            if (entity.IsOutOfService == isOutOfService)
            {
                if (isOutOfService)
                {
                    await UpdateOutOfServiceHistoryReason(entity.Id, reason);
                }
                return;
            }

            if (isOutOfService)
            {
                entity.OutOfServiceHistories.Add(new OutOfServiceHistory
                {
                    OutOfServiceDate = Clock.Now,
                    Reason = reason.Truncate(500),
                });
            }
            else
            {
                Debug.Assert(isOutOfService == false && entity.IsOutOfService == true);
                await _outOfServiceHistoryRepository.SetInServiceDate(entity.Id, Clock.Now);
            }
        }

        private async Task UpdateOutOfServiceHistoryReason(int truckId, string reason)
        {
            var outOfServiceHistory = await _outOfServiceHistoryRepository.GetAll()
                .Where(oosh => oosh.TruckId == truckId)
                .OrderByDescending(oosh => oosh.OutOfServiceDate)
                .FirstAsync();
            outOfServiceHistory.Reason = reason.Truncate(500);
        }
        private async Task<bool> RemoveTruckFromScheduleIfTruckIsOutOfService(int truckId, bool isOutOfService)
        {
            if (truckId == 0 || !isOutOfService)
            {
                return false;
            }

            return await RemoveTruckFromScheduleStartingOnDate(truckId);
        }

        private async Task RemoveTruckFromScheduleAndDriverAssignmentIfTruckIsNotIsActive(int truckId, bool isActive, DateTime? inactivationDate)
        {
            if (truckId == 0 || isActive)
            {
                return;
            }

            await RemoveTruckFromScheduleStartingOnDate(truckId, inactivationDate);
            await RemoveTruckFromDriverAssignmentStartingOnDate(truckId, inactivationDate);
        }

        private async Task UpdateTruckFiles(int truckId, List<TruckFileEditDto> truckFiles)
        {
            List<TruckFile> truckFileEntities = await _truckFileRepository.GetAllListAsync(tf => tf.TruckId == truckId);
            truckFileEntities.ForEach(entity =>
            {
                var dto = truckFiles.First(x => x.Id == entity.Id);
                entity.Title = dto.Title;
                entity.Description = dto.Description;
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<SetTruckIsOutOfServiceResult> SetTruckIsOutOfService(SetTruckIsOutOfServiceInput input)
        {
            var truck = await _truckRepository.GetAsync(input.TruckId);
            await CreateOrUpdateOutOfServiceHistory(truck, input.IsOutOfService, input.Reason);
            var result = new SetTruckIsOutOfServiceResult();
            if (input.IsOutOfService)
            {
                result.ThereWereAssociatedOrders = await RemoveTruckFromScheduleStartingOnDate(truck.Id);
                result.ThereWereCanceledDispatches = await CancelUnacknowledgedDispatches(truck.Id);
                result.ThereWereNotCanceledDispatches = await ThereAreAcknowledgedOrLoadedDispatches(truck.Id);
            }
            truck.IsOutOfService = input.IsOutOfService;
            return result;
        }

        private async Task<bool> RemoveTruckFromScheduleStartingOnDate(int truckId, DateTime? date = null)
        {
            var today = await GetToday();
            var startingDate = date ?? today;

            var orderLineTrucksToRemove = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.TruckId == truckId && olt.OrderLine.Order.DeliveryDate >= startingDate)
                .Select(olt => new
                {
                    olt.Id,
                    olt.OrderLine.Order.DeliveryDate
                })
                .ToListAsync();

            var orderLineTruckIdsToDelete = orderLineTrucksToRemove.Where(x => x.DeliveryDate != today).Select(x => x.Id).ToList();
            var orderLineTrucksToDelete = await _orderLineTruckRepository.GetAll()
                .Where(x => orderLineTruckIdsToDelete.Contains(x.Id) || x.ParentOrderLineTruckId.HasValue && orderLineTruckIdsToDelete.Contains(x.ParentOrderLineTruckId.Value))
                .ToListAsync();
            orderLineTrucksToDelete.ForEach(_orderLineTruckRepository.Delete);

            if (orderLineTrucksToRemove.Where(x => x.DeliveryDate == today).Any())
            {
                var ids = orderLineTrucksToRemove.Select(x => x.Id).ToList();
                var orderLineTrucksToComplete = await _orderLineTruckRepository.GetAll().Where(x => ids.Contains(x.Id) || x.ParentOrderLineTruckId.HasValue && ids.Contains(x.ParentOrderLineTruckId.Value)).ToListAsync();
                foreach (var orderLineTruck in orderLineTrucksToComplete)
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }
            }

            var orderLineIdsNeedingStaggeredTimeRecalculation = orderLineTrucksToDelete.Select(x => x.OrderLineId).Distinct().ToList();
            if (orderLineIdsNeedingStaggeredTimeRecalculation.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var orderLineId in orderLineIdsNeedingStaggeredTimeRecalculation.Distinct().ToList())
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                    var order = await orderLineUpdater.GetOrderAsync();
                    if (order.DeliveryDate >= today)
                    {
                        orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                        await orderLineUpdater.SaveChangesAsync();
                    }
                }
            }

            return orderLineTrucksToRemove.Count > 0;
        }

        private async Task<bool> RemoveTruckFromDriverAssignmentStartingOnDate(int truckId, DateTime? date)
        {
            var today = await GetToday();
            var startingDate = date ?? today;
            var driverAssignmentIdsToRemove = await _driverAssignmentRepository.GetAll()
                .Where(da => da.TruckId == truckId && da.Date >= startingDate && da.Date != today)
                .Select(da => da.Id)
                .ToListAsync();
            foreach (var driverAssignmentId in driverAssignmentIdsToRemove)
            {
                await _driverAssignmentRepository.DeleteAsync(driverAssignmentId);
            }

            return driverAssignmentIdsToRemove.Count > 0;
        }

        private async Task<bool> CancelUnacknowledgedDispatches(int truckId)
        {
            var syncRequest = new SyncRequest();
            var dispatchesToCancel = await _dispatchRepository.GetAll()
                .Where(d => d.TruckId == truckId && (d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Sent))
                .Select(d => d)
                .ToListAsync();
            foreach (var dispatch in dispatchesToCancel)
            {
                dispatch.Status = DispatchStatus.Canceled;
                dispatch.Canceled = Clock.Now;
                syncRequest.AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity(), ChangeType.Removed);
            }

            if (dispatchesToCancel.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var dispatchGroup in dispatchesToCancel.GroupBy(x => x.DriverId))
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                    {
                        LogMessage = $"Inactivated truckId {truckId} canceled unacknowledged dispatch(es) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(syncRequest
                        .AddLogMessage($"Inactivated truckId {truckId} canceled unacknowledged dispatch(es)"));
            }

            return dispatchesToCancel.Count > 0;
        }

        private async Task<bool> ThereAreAcknowledgedOrLoadedDispatches(int truckId) =>
            await _dispatchRepository.GetAll().AnyAsync(d => d.TruckId == truckId && (d.Status == DispatchStatus.Acknowledged || d.Status == DispatchStatus.Loaded));

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<bool> CanDeleteTruck(EntityDto input)
        {
            bool hasDependencies = await _truckRepository.GetAll()
                .Where(t => t.Id == input.Id)
                .Where(t =>
                    t.OrderLineTrucksOfTruck.Any() ||
                    t.OrderLineTrucksOfTrailer.Any() ||
                    t.Tickets.Any(ticket => ticket.CarrierId == null) ||
                    t.PreventiveMaintenances.Any() ||
                    t.DriverAssignments.Any() ||
                    t.SharedTrucks.Any() ||
                    t.WorkOrders.Any()
                )
                .AnyAsync();

            return !hasDependencies;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task DeleteTruck(EntityDto input)
        {
            var canDelete = await CanDeleteTruck(input);
            if (!canDelete)
            {
                throw new UserFriendlyException(L("UnableToDeleteTruckWithAssociatedData"));
            }
            await _truckRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<AddSharedTruckListDto> GetAddSharedTruckModel(GetAddSharedTruckModelInput input)
        {
            var truck = await _truckRepository.GetAll()
                .Where(x => x.Id == input.TruckId)
                .Select(x => new
                {
                    OfficeId = x.LocationId
                })
                .FirstAsync();

            var offices = await _officeRepository.GetAll()
                .Where(x => x.Id != truck.OfficeId)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                })
                .ToListAsync();

            return new AddSharedTruckListDto
            {
                TruckId = input.TruckId,
                Offices = offices
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task AddSharedTruck(AddSharedTruckInput input)
        {
            await CheckUseShiftSettingCorrespondsInput(input.Shifts);
            Shift?[] shifts = input.Shifts.ToNullableArrayWithNullElementIfEmpty();

            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;

            if (input.EndDate < input.StartDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            var existingShares = await _sharedTruckRepository.GetAll()
                .Where(x => x.Date >= input.StartDate && x.Date <= input.EndDate && x.TruckId == input.TruckId)
                .Select(x => new
                {
                    x.Date,
                    x.OfficeId
                })
                .ToListAsync();

            var date = input.StartDate;
            while (date <= input.EndDate)
            {
                foreach (var shift in shifts)
                {
                    await ThrowUserFriendlyExceptionIfTruckIsScheduled(input.TruckId, date, shift);
                    var existingShare = existingShares.FirstOrDefault(x => x.Date == date && x.OfficeId == input.OfficeId);
                    if (existingShare == null)
                    {
                        await _sharedTruckRepository.InsertAsync(new SharedTruck
                        {
                            OfficeId = input.OfficeId,
                            Date = date,
                            TruckId = input.TruckId,
                            Shift = shift,
                        });
                        (await _driverAssignmentRepository.GetAll()
                            .Where(x => x.Date == date && x.TruckId == input.TruckId && x.Shift == shift)
                            .ToListAsync())
                            .ForEach(x => x.OfficeId = input.OfficeId);
                    }
                }
                date = date.AddDays(1);
            }

            // Local functions
            async Task ThrowUserFriendlyExceptionIfTruckIsScheduled(int truckId, DateTime scheduleDate, Shift? shift)
            {
                if (await _orderLineTruckRepository.GetAll().AnyAsync(olt => olt.TruckId == truckId && olt.OrderLine.Order.DeliveryDate == scheduleDate && olt.OrderLine.Order.Shift == shift && !olt.IsDone && !olt.OrderLine.IsComplete))
                {
                    string shiftNameOrEmptyString = shift == null ? "" : " and shift " + await SettingManager.GetShiftName(shift);
                    throw new UserFriendlyException($"Truck cannot be shared because it is scheduled for {scheduleDate:d}{shiftNameOrEmptyString}");
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task DeleteSharedTruck(DeleteSharedTruckInput input)
        {
            input.Date = input.Date.Date;

            var shares = await _sharedTruckRepository.GetAll()
                .Where(x => x.Date == input.Date && x.TruckId == input.TruckId)
                .ToListAsync();

            shares.ForEach(async truck => await _sharedTruckRepository.DeleteAsync(truck));

            (await _driverAssignmentRepository.GetAll()
                            .Where(x => x.Date == input.Date && x.TruckId == input.TruckId)
                            .ToListAsync())
                            .ForEach(x => x.OfficeId = OfficeId); //if CanEditTruck didn't throw an exception, the truck belongs to the current office
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<PagedResultDto<PreventiveMaintenanceByTruckDto>> GetPreventiveMaintenanceDueByTruck(int truckId)
        {
            var items = await _truckRepository.GetAll()
                .Where(t => t.Id == truckId)
                .SelectMany(t => t.PreventiveMaintenances)
                .Select(pm => new PreventiveMaintenanceByTruckDto
                {
                    VehicleServiceName = pm.VehicleService.Name,
                    DueDate = pm.DueDate,
                    DueMileage = pm.DueMileage,
                    DueHour = pm.DueHour,
                })
                .ToListAsync();

            return new PagedResultDto<PreventiveMaintenanceByTruckDto>(items.Count, items);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<PagedResultDto<TruckServiceHistoryDto>> GetServiceHistoryByTruck(int truckId)
        {
            var items = await _truckRepository.GetAll()
                .Where(t => t.Id == truckId)
                .SelectMany(t => t.WorkOrders)
                .Where(wo => wo.CompletionDate.HasValue)
                .SelectMany(wo => wo.WorkOrderLines)
                .Select(wol => new TruckServiceHistoryDto
                {
                    VehicleServiceName = wol.VehicleService.Name,
                    CompletionDate = wol.WorkOrder.CompletionDate.Value,
                    Mileage = wol.WorkOrder.Odometer,
                })
                .ToListAsync();

            return new PagedResultDto<TruckServiceHistoryDto>(items.Count, items);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task SaveCurrentMileageAndHours(SaveCurrentMileageInput input)
        {
            Truck entity = await _truckRepository.GetAsync(input.TruckId);
            entity.CurrentMileage = input.CurrentMileage;
            entity.CurrentHours = input.CurrentHours;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<TruckFileEditDto> SaveFile(TruckFileEditDto model)
        {
            TruckFile entity = model.Id != 0 ? await _truckFileRepository.GetAsync(model.Id) : new TruckFile();

            entity.TruckId = model.TruckId;
            entity.FileId = model.FileId;
            entity.ThumbnailId = model.ThumbnailId;
            entity.FileName = model.FileName;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.FileType = model.FileType;

            model.Id = await _truckFileRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<TruckFileEditDto> GetTruckFileEditDto(int id)
        {
            return await _truckFileRepository.GetAll()
                .Select(x => new TruckFileEditDto()
                {
                    Id = x.Id,
                    TruckId = x.TruckId,
                    Title = x.Title,
                    Description = x.Description,
                    FileId = x.FileId,
                    ThumbnailId = x.ThumbnailId,
                    FileName = x.FileName,
                    FileType = x.FileType,
                }
                )
                .SingleAsync(x => x.Id == id);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task DeleteFile(EntityDto input)
        {
            var entity = await _truckFileRepository.GetAsync(input.Id);
            AttachmentHelper.DeleteFromAzureBlob($"{entity.TruckId}/{entity.FileId}", AppConsts.TruckFilesContainerName);
            await _truckFileRepository.DeleteAsync(entity);
        }

        public async Task UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput input)
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            var originalMaxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
            await TenantManager.SetFeatureValueAsync(Session.TenantId ?? 0, AppFeatures.NumberOfTrucksFeature, input.NewValue.ToString());

            var notificationsEmail = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.NotificationsEmail);
            var tenantName = (await TenantManager.GetByIdAsync(Session.TenantId ?? 0)).TenancyName;
            var body = $"Tenant {tenantName} increased their trucks to {input.NewValue}. Their old limit was {originalMaxNumberOfTrucks}";
            Logger.Info(body);

            try
            {
                await _emailSender.SendAsync(new MailMessage(await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress), notificationsEmail)
                {
                    Subject = "Tenant increased their trucks number",
                    Body = body,
                    IsBodyHtml = false
                });
            }
            catch (Exception e)
            {
                Logger.Error("Error during sending a email for UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins", e);
                //don't rethrow, send a notification too
            }

            await _appNotifier.SendNotificationAsync(
                        new SendNotificationInput(
                            AppNotificationNames.SimpleMessage,
                            body,
                            NotificationSeverity.Warn
                        )
                        {
                            IncludeLocalUsers = false,
                            IncludeHostUsers = true,
                            RoleFilter = new[] { StaticRoleNames.Host.Admin }
                        });
        }

    }
}

