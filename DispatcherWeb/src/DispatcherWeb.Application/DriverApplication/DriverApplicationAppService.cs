using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Timing;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DispatcherWeb.DriverApplication
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication)]
    public class DriverApplicationAppService : DispatcherWebAppServiceBase, IDriverApplicationAppService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<DriverApplicationDevice> _deviceRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IDriverApplicationLogRepository _driverApplicationLogRepository;
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IDriverApplicationAuthProvider _driverApplicationAuthProvider;
        private readonly IPushSubscriptionManager _pushSubscriptionManager;
        private readonly IConfigurationRoot _appConfiguration;

        public DriverApplicationAppService(
            IRepository<OrderLine> orderLineRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Driver> driverRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<DriverApplicationDevice> deviceRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IDriverApplicationLogRepository driverApplicationLogRepository,
            ITimeZoneConverter timeZoneConverter,
            IDriverApplicationAuthProvider driverApplicationAuthProvider,
            IPushSubscriptionManager pushSubscriptionManager,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _orderLineRepository = orderLineRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _driverRepository = driverRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _deviceRepository = deviceRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _driverApplicationLogRepository = driverApplicationLogRepository;
            _timeZoneConverter = timeZoneConverter;
            _driverApplicationAuthProvider = driverApplicationAuthProvider;
            _pushSubscriptionManager = pushSubscriptionManager;
            _appConfiguration = configurationAccessor.Configuration;
        }

        //driver application 1
        public async Task<DateTime?> GetScheduledStartTime()
        {
            var driverId = await GetCurrentDriverIdOrThrow();

            DateTime date = await GetToday();
            return (await _driverAssignmentRepository.GetAll()
                .Where(da => da.DriverId == driverId && da.Date == date)
                .OrderBy(da => da.StartTime)
                .Select(da => da.StartTime)
                .FirstOrDefaultAsync())?.ConvertTimeZoneTo(await GetTimezone());
        }

        [AbpAllowAnonymous]
        public async Task<List<ScheduledStartTimeInfo>> GetScheduledStartTimeInfo(GetScheduledStartTimeInfoInput input)
        {
            var driverId = await _driverApplicationAuthProvider.GetDriverIdFromSessionOrGuid(input.DriverGuid);
            var today = await GetToday();
            var tomorrow = today.AddDays(1);
            var result = new List<ScheduledStartTimeInfo>
            {
                await GetScheduledStartTimeInfoFor(driverId, today),
                await GetScheduledStartTimeInfoFor(driverId, tomorrow)
            };

            foreach (var assignment in result.ToList())
            {
                if (assignment.NextAssignmentDate.HasValue && !result.Any(r => r.Date == assignment.NextAssignmentDate))
                {
                    result.Add(await GetScheduledStartTimeInfoFor(driverId, assignment.NextAssignmentDate.Value));
                }
            }

            return result;
        }

        private async Task<ScheduledStartTimeInfo> GetScheduledStartTimeInfoFor(int driverId, DateTime date)
        {
            var info = await _driverAssignmentRepository.GetAll()
                .Where(da => da.DriverId == driverId && da.Date == date)
                .OrderBy(da => da.StartTime == null)
                //.ThenBy(da => da.StartTimeObsolete.HasValue ? da.StartTimeObsolete.Value.TimeOfDay : (TimeSpan?)null)
                .ThenBy(da => da.StartTime)
                .Select(da => new ScheduledStartTimeInfo
                {
                    Date = date,
                    StartTime = da.StartTime,
                    TruckCode = da.Truck.TruckCode,
                    HasDriverAssignment = true
                })
                .FirstOrDefaultAsync();

            var timezone = await GetTimezone();

            if (info == null)
            {
                info = await _driverRepository.GetAll()
                    .Where(x => x.Id == driverId)
                    .Select(x => new ScheduledStartTimeInfo
                    {
                        Date = date,
                        TruckCode = x.DefaultTrucks.FirstOrDefault().TruckCode,
                        HasDriverAssignment = false
                    }).FirstAsync();
            }

            if (info.StartTime.HasValue)
            {
                info.StartTime = info.Date.Date.Add(info.StartTime.Value.ConvertTimeZoneTo(timezone).TimeOfDay);
            }

            var nextInfo = await _driverAssignmentRepository.GetAll()
                .Where(da => da.DriverId == driverId && da.Date > date)
                .OrderBy(da => da.Date)
                .Select(da => new
                {
                    Date = da.Date
                })
                .FirstOrDefaultAsync();

            info.NextAssignmentDate = nextInfo?.Date;

            return info;
        }

        public async Task<bool> UserIsClockedIn()
        {
            return await _employeeTimeRepository.GetAll()
                .AnyAsync(et => et.UserId == AbpSession.UserId.Value && et.EndDateTime == null && !et.IsImported);
        }

        public async Task<GetDriverGuidResult> GetDriverGuid()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.UserId?.ToString());
            var result = new GetDriverGuidResult
            {
                UserId = user.Id,
                IsAdmin = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin) 
                    || await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Administrative),
                DriverName = user.Name + " " + user.Surname
            };

            var driver = await _driverRepository.GetAll()
                .Include(x => x.LeaseHaulerDriver)
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync();

            if (driver == null)
            {
                return result;
            }

            if (driver.Guid == null)
            {
                driver.Guid = Guid.NewGuid();
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            result.IsDriver = true;
            result.DriverGuid = driver.Guid.Value;
            result.DriverId = driver.Id;
            result.DriverName = driver.FirstName + " " + driver.LastName;
            result.DriverLeaseHaulerId = driver.LeaseHaulerDriver?.LeaseHaulerId;

            return result;
        }

        private async Task<int> GetCurrentDriverIdOrDefault()
        {
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            return await _driverRepository.GetDriverIdByUserIdOrDefault(AbpSession.UserId.Value);
        }

        private async Task<int> GetCurrentDriverIdOrThrow()
        {
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            return await _driverRepository.GetDriverIdByUserIdOrThrow(AbpSession.UserId.Value);
        }

        public async Task CreateEmployeeTime(CreateEmployeeTimeInput input)
        {
            DateTime userDate = await GetToday();
            var utcToday = Clock.Now.Date;
            int driverId = await GetCurrentDriverIdOrThrow();
            var userId = GetUserId();

            //NotEndedTodayEmployeeTimeExists
            if (await _employeeTimeRepository.GetAll()
                    .AnyAsync(et => et.UserId == userId && et.StartDateTime >= utcToday && et.EndDateTime == null && !et.IsImported))
            {
                return;
            }

            await SetPreviousEmployeeTimeEndDateTimeIfNull();

            var timeClassificationId = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, AbpSession.TenantId ?? 0); //todo

            var employeeTime = new Drivers.EmployeeTime
            {
                UserId = userId,
                StartDateTime = Clock.Now,
                TimeClassificationId = timeClassificationId,
                EquipmentId = await GetTruckId(),
                Latitude = input.Latitude,
                Longitude = input.Longitude
            };
            await _employeeTimeRepository.InsertAsync(employeeTime);

            // Local functions
            async Task<int?> GetTruckId() =>
                await _driverAssignmentRepository.GetAll()
                    .Where(da => da.DriverId == driverId && da.Date == userDate)
                    .Select(da => (int?)da.TruckId)
                    .FirstOrDefaultAsync();

            async Task SetPreviousEmployeeTimeEndDateTimeIfNull()
            {
                var notEndedEmployeeTime = await GetNotEndedEmployeeTime(userId);
                if (notEndedEmployeeTime == null)
                {
                    return;
                }

                notEndedEmployeeTime.EndDateTime = ConvertToLocalDateTime(notEndedEmployeeTime.StartDateTime).EndOfDay();

                DateTime ConvertToLocalDateTime(DateTime dateTime)
                {
                    Debug.Assert(AbpSession.TenantId != null, "AbpSession.TenantId != null");
                    return _timeZoneConverter.Convert(dateTime, AbpSession.TenantId.Value) ?? dateTime;
                }
            }
        }

        public async Task SetEmployeeTimeEndDateTime()
        {
            var notEndedEmployeeTime = await GetNotEndedEmployeeTime(GetUserId());
            if (notEndedEmployeeTime == null)
            {
                throw new ApplicationException("There is no not finished EmployeeTime!");
            }

            notEndedEmployeeTime.EndDateTime = Clock.Now;
        }

        private async Task<Drivers.EmployeeTime> GetNotEndedEmployeeTime(long userId) =>
            await _employeeTimeRepository.GetAll()
                .FirstOrDefaultAsync(et => et.UserId == userId && et.EndDateTime == null && !et.IsImported);

        private long GetUserId()
        {
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            var userId = AbpSession.UserId.Value;
            return userId;
        }

        public async Task<DriverAppInfo> PostDriverAppInfo(GetDriverAppInfoInput input)
        {
            return await GetDriverAppInfo(input);
        }

        private int GetDriverApplicationHttpTimeout()
        {
            const int defaultTimeout = 60000;
            var httpTimeoutString = _appConfiguration["App:DriverApplicationHttpRequestTimeout"];
            if (!string.IsNullOrEmpty(httpTimeoutString) && int.TryParse(httpTimeoutString, out var httpTimeout))
            {
                return httpTimeout;
            }
            return defaultTimeout;
        }

        public async Task<DriverAppInfo> GetDriverAppInfo(GetDriverAppInfoInput input)
        {
            var driverGuidInfo = await GetDriverGuid();
            var result = new DriverAppInfo
            {
                ElapsedTime = await GetElapsedTime(),
                UseShifts = await SettingManager.UseShifts(),
                UseBackgroundSync = (_appConfiguration["App:UseBackgroundSyncForDriverApp"] ?? "true") != "false",
                HttpRequestTimeout = GetDriverApplicationHttpTimeout(),
                ShiftNames = (await SettingManager.GetShiftDictionary()).ToDictionary(x => (int)x.Key, x => x.Value),
                DriverGuid = driverGuidInfo.DriverGuid,
                DriverName = driverGuidInfo.DriverName,
                DriverLeaseHaulerId = driverGuidInfo.DriverLeaseHaulerId,
                IsDriver = driverGuidInfo.IsDriver,
                IsAdmin = driverGuidInfo.IsAdmin,
                UserId = driverGuidInfo.UserId,
                HideTicketControls = await SettingManager.HideTicketControlsInDriverApp(),
                RequireToEnterTickets = await SettingManager.RequireDriversToEnterTickets(),
                RequireSignature = await SettingManager.RequireSignature(),
                RequireTicketPhoto = await SettingManager.RequireTicketPhoto(),
                TextForSignatureView = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView),
                DispatchesLockedToTruck = await SettingManager.DispatchesLockedToTruck(),
            };

            if (input.RequestNewDeviceId)
            {
                result.DeviceId = await _deviceRepository.InsertAndGetIdAsync(new DriverApplicationDevice
                {
                    Useragent = input.Useragent 
                });
            }

            result.ProductionPayId = await _timeClassificationRepository.GetAll()
                .Where(x => x.IsProductionBased)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (driverGuidInfo.DriverId != 0)
            {
                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

                result.TimeClassifications = await _employeeTimeClassificationRepository.GetAll()
                    .Where(x => x.DriverId == driverGuidInfo.DriverId)
                    .WhereIf(!allowProductionPay, x => !x.TimeClassification.IsProductionBased)
                    .Select(x => new TimeClassificationDto
                    {
                        Id = x.TimeClassificationId,
                        Name = x.TimeClassification.Name,
                        IsDefault = x.IsDefault
                    })
                    .OrderByDescending(x => x.IsDefault)
                    .ThenBy(x => x.Name)
                    .ToListAsync();
            }

            if (input.PushSubscription != null && driverGuidInfo.DriverId != 0)
            {
                await _pushSubscriptionManager.AddDriverPushSubscription(new AddDriverPushSubscriptionInput
                {
                    PushSubscription = input.PushSubscription,
                    DriverId = driverGuidInfo.DriverId,
                    DeviceId = input.DeviceId ?? result.DeviceId
                });
            }

            return result;
        }

        public async Task<GetElapsedTimeResult> GetElapsedTime()
        {
            //var today = Clock.Now.Date;
            var timeZone = await GetTimezone();
            var todayInLocal = await GetToday();
            var todayInUtc = todayInLocal.ConvertTimeZoneFrom(timeZone);
            var driverTimes = await _employeeTimeRepository.GetAll()
                .Where(et => et.UserId == GetUserId() && et.StartDateTime >= todayInUtc && (et.EndDateTime == null || et.EndDateTime < todayInUtc.AddDays(1)))
                .Select(x => new
                {
                    x.StartDateTime,
                    x.EndDateTime,
                    x.IsImported
                })
                .ToListAsync();
            //var elapsedSeconds = driverTimes.Sum(x => ((x.EndDateTime ?? Clock.Now) - x.StartDateTime).TotalSeconds);
            var committedElapsedSeconds = driverTimes.Where(x => x.EndDateTime != null).Sum(x => (x.EndDateTime.Value - x.StartDateTime).TotalSeconds);
            var uncommittedElapsedSeconds = driverTimes.Where(x => x.EndDateTime == null && !x.IsImported).Sum(x => (Clock.Now - x.StartDateTime).TotalSeconds);
            //var committedElapsedTime = todayInLocal.AddSeconds(committedElapsedSeconds);
            var totalElapsedTime = todayInLocal.AddSeconds(committedElapsedSeconds + uncommittedElapsedSeconds);
            return new GetElapsedTimeResult
            {
                //DriverApplication1
                ElapsedTime = totalElapsedTime,
                //DriverApplication1&2
                ClockIsStarted = driverTimes.Any(x => x.EndDateTime == null && !x.IsImported),
                //DriverApplication2
                CommittedElapsedSeconds = committedElapsedSeconds,
                CommittedElapsedSecondsForDay = todayInLocal,
                LastClockStartTime = driverTimes.FirstOrDefault(x => x.EndDateTime == null && !x.IsImported)?.StartDateTime.ConvertTimeZoneTo(timeZone)
            };
        }

        [AbpAllowAnonymous]
        public async Task<List<EmployeeTimeSlimDto>> GetEmployeeTimesForCurrentUser(GetEmployeeTimesForCurrentUserInput input)
        {
            var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuidIfNeeded(input.DriverGuid);

            var timezone = await GetTimezone();
            input.FromDate = input.FromDate.ConvertTimeZoneFrom(timezone);
            input.ToDate = input.ToDate.ConvertTimeZoneFrom(timezone);

            var items = await _employeeTimeRepository.GetAll()
                .Where(x => x.StartDateTime >= input.FromDate)
                .Where(x => x.StartDateTime < input.ToDate)
                .Where(x => x.UserId == authInfo.UserId)
                .WhereIf(input.UpdatedAfterDateTime.HasValue, x => x.CreationTime > input.UpdatedAfterDateTime.Value || (x.LastModificationTime != null && x.LastModificationTime > input.UpdatedAfterDateTime.Value))
                .Select(x => new EmployeeTimeSlimDto
                {
                    Id = x.Id,
                    Guid = x.Guid,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    TimeClassificationId = x.TimeClassificationId,
                    //EquipmentId = x.EquipmentId,
                    LastUpdateDateTime = x.LastModificationTime.HasValue && x.LastModificationTime.Value > x.CreationTime ? x.LastModificationTime.Value : x.CreationTime,
                    IsEditable = x.PayStatementTime == null
                }).ToListAsync();

            foreach (var item in items)
            {
                item.StartDateTime = item.StartDateTime?.ConvertTimeZoneTo(timezone);
                item.EndDateTime = item.EndDateTime?.ConvertTimeZoneFrom(timezone);
            }

            return items;
        }

        [UnitOfWork(false)]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        public void RemoveOldDriverApplicationLogs()
        {
            try
            {
                //var targetDate = Clock.Now.AddDays(-15).Date;
                Logger.Info($"RemoveOldDriverApplicationLogs started at {Clock.Now:s}");


                using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions
                {
                    IsTransactional = false,
                    Timeout = TimeSpan.FromMinutes(60),
                }))
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    //_driverApplicationLogRepository.DeleteLogsEarlierThan(targetDate);
                    _driverApplicationLogRepository.DeleteOldLogs();

                    unitOfWork.Complete();
                }
                Logger.Info($"RemoveOldDriverApplicationLogs finished at {Clock.Now:s}");
            }
            catch (Exception e)
            {
                Logger.Error("RemoveOldDriverApplicationLogs failed", e);
                throw;
            }
        }
    }
}
