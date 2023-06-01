using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.Dashboard.Dto;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Dashboard.RevenueGraph.Factories;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dashboard
{
    [AbpAuthorize(AppPermissions.Pages_Dashboard)]
    public class DashboardAppService : DispatcherWebAppServiceBase, IDashboardAppService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<TenantDailyHistory> _tenantDailyHistoryRepository;
        private readonly IRepository<FuelPurchase> _fuelPurchaseRepository;
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;

        private readonly ITruckTelematicsAppService _truckTelematicsAppService;
        private readonly IDashboardSettingManager _dashboardSettingManager;
        private readonly IRevenueGraphByTicketsDataItemsQueryService _revenueGraphByTicketsDataItemsQueryService;

        public DashboardAppService(
            IRepository<OrderLine> orderLineRepository,
            IRepository<Truck> truckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<TenantDailyHistory> tenantDailyHistory,
            IRepository<FuelPurchase> fuelPurchaseRepository,
            IRepository<VehicleUsage> vehicleUsageRepository,
            ITruckTelematicsAppService truckTelematicsAppService,
            IDashboardSettingManager dashboardSettingManager,
            IRevenueGraphByTicketsDataItemsQueryService revenueGraphByTicketsDataItemsQueryService
        )
        {
            _orderLineRepository = orderLineRepository;
            _truckRepository = truckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _driverRepository = driverRepository;
            _ticketRepository = ticketRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _tenantDailyHistoryRepository = tenantDailyHistory;
            _fuelPurchaseRepository = fuelPurchaseRepository;
            _vehicleUsageRepository = vehicleUsageRepository;
            _truckTelematicsAppService = truckTelematicsAppService;
            _dashboardSettingManager = dashboardSettingManager;
            _revenueGraphByTicketsDataItemsQueryService = revenueGraphByTicketsDataItemsQueryService;
        }

        public async Task<ScheduledTruckCountDto> GetScheduledTruckCountDto()
        {
            if (!Session.OfficeId.HasValue)
            {
                throw new UserFriendlyException("You don't have an office assigned.", "Ask your administrator to set your office and you will be able to see more on the dashboard.");
            }

            var date = await GetToday();
            var tomorrow = date.AddDays(1);
            var orderLines = _orderLineRepository.GetAll()
                .Where(ol => ol.Order.LocationId == OfficeId || ol.SharedOrderLines.Any(s => s.OfficeId == OfficeId));
            var todayOrderLines = orderLines.Where(ol => ol.Order.DeliveryDate == date && !ol.Order.IsPending);
            var tomorrowOrderLines = orderLines.Where(ol => ol.Order.DeliveryDate == tomorrow && !ol.Order.IsPending);

            var result = new ScheduledTruckCountDto
            {
                TrucksRequestedTodayCount = await todayOrderLines
                                                .Where(ol => !ol.IsComplete)
                                                .Select(ol => ol.NumberOfTrucks ?? 0)
                                                .SumAsync(),
                TrucksScheduledForTodayCount = await todayOrderLines
                                                .Where(ol => !ol.IsComplete)
                                                .Select(ol => ol.ScheduledTrucks ?? 0)
                                                .SumAsync(),
                TrucksRequestedTomorrowCount = await tomorrowOrderLines
                                                .Where(ol => !ol.IsComplete)
                                                .Select(ol => ol.NumberOfTrucks ?? 0)
                                                .SumAsync(),
                TrucksScheduledForTomorrowCount = await tomorrowOrderLines
                                                .Where(ol => !ol.IsComplete)
                                                .Select(ol => ol.ScheduledTrucks ?? 0)
                                                .SumAsync(),
            };

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_Revenue)]
        public async Task<GetRevenueGraphDataOutput> GetRevenueByDateGraphData(GetRevenueByDateGraphDataInput input)
        {
            var revenueGraphDataService =
                RevenueGraphDataServiceFactory.CreateRevenueGraphDataService(
                    input.DatePeriod,
                    _revenueGraphByTicketsDataItemsQueryService
                );
            return await revenueGraphDataService.GetRevenueGraphData(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_TruckUtilization)]
        public async Task<GetTruckUtilizationDataOutput> GetTruckUtilizationData(GetTruckUtilizationDataInput input)
        {
            var utilizedTrucks = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.Truck.LocationId == OfficeId && olt.OrderLine.Order.DeliveryDate >= input.PeriodBegin && olt.OrderLine.Order.DeliveryDate <= input.PeriodEnd)
                .Where(olt => olt.Truck.VehicleCategory.IsPowered && olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule != true && olt.Truck.LocationId != null)
                .Select(olt => new { olt.OrderLine.Order.DeliveryDate, olt.TruckId })
                .Distinct()
                .Select(x => new TruckUtilizationOnDate { DeliveryDate = x.DeliveryDate, Utilization = 1 }).ToListAsync()
            ;

            var activeTrucks = _truckRepository.GetAll()
                .Where(t => t.LocationId == OfficeId)
                .Where(t => t.VehicleCategory.IsPowered && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true && t.LocationId != null)
                .Where(t =>
                    t.InServiceDate <= input.PeriodEnd &&
                    (t.InactivationDate == null || t.InactivationDate > input.PeriodBegin) &&
                    (t.SoldDate == null || t.SoldDate > input.PeriodBegin)
                )
                .Select(t => new
                {
                    t.InServiceDate,
                    InactivationDate =
                        t.InactivationDate == null ? t.SoldDate :
                        t.SoldDate == null ? t.InactivationDate :
                        t.InactivationDate < t.SoldDate ? t.InactivationDate : t.SoldDate,
                })
                .Select(t => new TruckActivePeriod
                {
                    ActiveFrom = t.InServiceDate <= input.PeriodBegin ? input.PeriodBegin : t.InServiceDate,
                    ActiveTo = t.InactivationDate == null || t.InactivationDate > input.PeriodEnd ? input.PeriodEnd : t.InactivationDate.Value.AddDays(-1),
                })
            ;
            var activeTrucksByPeriod = await
                (from at in activeTrucks
                 group at by new { at.ActiveFrom, at.ActiveTo }
                    into gt
                 select new TruckActivePeriod
                 {
                     ActiveFrom = gt.Key.ActiveFrom,
                     ActiveTo = gt.Key.ActiveTo,
                     Number = gt.Sum(x => 1),
                 })
                .ToListAsync();


            List<TruckUtilizationData> truckUtilizationForPeriod;
            switch (input.DatePeriod)
            {
                case TruckUtilizationDatePeriod.Daily:
                    truckUtilizationForPeriod = GetTruckUtilizationByDay(utilizedTrucks, activeTrucksByPeriod, input);
                    break;
                case TruckUtilizationDatePeriod.Weekly:
                    truckUtilizationForPeriod = GetTruckUtilizationByWeek(utilizedTrucks, activeTrucksByPeriod, input);
                    break;
                case TruckUtilizationDatePeriod.Monthly:
                    truckUtilizationForPeriod = GetTruckUtilizationByMonth(utilizedTrucks, activeTrucksByPeriod, input);
                    break;
                default:
                    throw new ArgumentException($"Not supported Date Period: {input.DatePeriod}");
            }

            return new GetTruckUtilizationDataOutput(truckUtilizationForPeriod);
        }

        private List<TruckUtilizationData> GetTruckUtilizationByDay(
            List<TruckUtilizationOnDate> utilizedTrucks,
            List<TruckActivePeriod> activeTrucksByPeriod,
            GetTruckUtilizationDataInput input
        )
        {
            var truckUtilization = from ot in utilizedTrucks
                                   group ot by ot.DeliveryDate
                    into gd
                                   select new
                                   {
                                       BeginOfPeriod = gd.Key,
                                       Utilization = gd.Sum(ot => ot.Utilization),
                                   };

            var truckUtilizationForPeriod = new List<TruckUtilizationData>();
            DateTime currentDate = input.PeriodBegin;
            while (currentDate <= input.PeriodEnd)
            {
                int numberOfTrucks = activeTrucksByPeriod.Where(x => x.ActiveFrom <= currentDate && x.ActiveTo >= currentDate).Sum(x => x.Number);
                decimal utilization = truckUtilization.Where(x => x.BeginOfPeriod == currentDate).Select(x => x.Utilization).FirstOrDefault();
                truckUtilizationForPeriod.Add(new TruckUtilizationData(
                    (int)Math.Round((numberOfTrucks != 0 ? utilization / numberOfTrucks : 0) * 100),
                    currentDate.ToString("yyyy-MM-dd")));
                currentDate = currentDate.AddDays(1);
            }

            return truckUtilizationForPeriod;
        }

        private List<TruckUtilizationData> GetTruckUtilizationByWeek(
            List<TruckUtilizationOnDate> utilizedTrucks,
            List<TruckActivePeriod> activeTrucksByPeriod,
            GetTruckUtilizationDataInput input
        )
        {
            DateTime firstDayOfTheWeek = input.PeriodBegin.StartOfWeek();
            var truckUtilization =
                from ot in utilizedTrucks
                group ot by new { Week = (ot.DeliveryDate.Value - firstDayOfTheWeek).Days / 7 }
                    into gd
                select new
                {
                    gd.Key.Week,
                    Utilization = gd.Sum(ot => ot.Utilization),
                };

            var truckUtilizationForPeriod = new List<TruckUtilizationData>();
            DateTime currentWeekStart = input.PeriodBegin;
            while (currentWeekStart <= input.PeriodEnd)
            {
                var currentWeekEnd = currentWeekStart.EndOfWeek();
                if (currentWeekEnd > input.PeriodEnd)
                {
                    currentWeekEnd = input.PeriodEnd;
                }
                int currentWeek = (currentWeekStart - firstDayOfTheWeek).Days / 7;
                int numberOfTrucks = GetNumberOfTrucks(currentWeekStart, currentWeekEnd, activeTrucksByPeriod);
                decimal utilization = truckUtilization.Where(x => x.Week == currentWeek).Select(x => x.Utilization).FirstOrDefault();
                truckUtilizationForPeriod.Add(new TruckUtilizationData(
                    (int)Math.Round((numberOfTrucks != 0 ? utilization / numberOfTrucks : 0) * 100),
                    $"{currentWeekStart:d} - {currentWeekEnd:d}"));
                currentWeekStart = currentWeekStart.StartOfWeek().AddDays(7);
            }

            return truckUtilizationForPeriod;

        }
        private int GetNumberOfTrucks(DateTime startDate, DateTime endDate, List<TruckActivePeriod> activeTrucksByPeriod)
        {
            int n = 0;
            while (startDate <= endDate)
            {
                var d = startDate;
                n += activeTrucksByPeriod.Where(x => x.ActiveFrom <= d && x.ActiveTo >= d).Sum(x => x.Number);
                startDate = startDate.AddDays(1);
            }
            return n;
        }

        private List<TruckUtilizationData> GetTruckUtilizationByMonth(
            List<TruckUtilizationOnDate> orderTrucks,
            List<TruckActivePeriod> activeTrucksByPeriod,
            GetTruckUtilizationDataInput input
        )
        {
            var truckUtilization = from ot in orderTrucks
                                   group ot by new { ot.DeliveryDate.Value.Month, ot.DeliveryDate.Value.Year }
                    into gd
                                   select new
                                   {
                                       BeginOfPeriod = new DateTime(gd.Key.Year, gd.Key.Month, 1),
                                       Utilization = gd.Sum(ot => ot.Utilization),
                                   };

            var truckUtilizationForPeriod = new List<TruckUtilizationData>();
            DateTime currentDate = input.PeriodBegin;
            while (currentDate <= input.PeriodEnd)
            {
                int numberOfTrucks = GetNumberOfTrucks(currentDate, currentDate.AddMonths(1).AddDays(-1), activeTrucksByPeriod);
                decimal utilization = truckUtilization.Where(x => x.BeginOfPeriod.Year == currentDate.Year && x.BeginOfPeriod.Month == currentDate.Month).Select(x => x.Utilization).FirstOrDefault();
                truckUtilizationForPeriod.Add(new TruckUtilizationData(
                    (int)Math.Round((numberOfTrucks != 0 ? utilization / numberOfTrucks : 0) * 100),
                    currentDate.ToString("MMMM yyyy")));
                currentDate = currentDate.AddMonths(1);
            }

            return truckUtilizationForPeriod;
        }

        public async Task SetSettingValue(string settingName, string value)
        {
            switch (settingName)
            {
                case AppSettings.GettingStarted.ShowGettingStarted:
                case AppSettings.GettingStarted.UsersChecked:
                case AppSettings.GettingStarted.DriversChecked:
                case AppSettings.GettingStarted.TrucksChecked:
                case AppSettings.GettingStarted.CustomersChecked:
                case AppSettings.GettingStarted.ServicesChecked:
                case AppSettings.GettingStarted.LocationsChecked:
                case AppSettings.GettingStarted.LeaseHaulersChecked:
                case AppSettings.GettingStarted.LogoChecked:
                    break;
                default:
                    throw new ApplicationException("Not authorized to change non-dashboard related settings");
            }

            await SettingManager.ChangeSettingForTenantAsync(Session.TenantId ?? 0, settingName, value);
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_DriverDotRequirements)]
        public async Task<GetTruckAvailabilityDataOutput> GetTruckAvailabilityData()
        {
            var query = _truckRepository.GetAll()
                .Where(t => t.IsActive && t.LocationId != null && t.VehicleCategory.IsPowered && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true);

            var availableTrucks = await query.Where(t => !t.IsOutOfService).CountAsync();
            var outOfServiceTrucks = await query.Where(t => t.IsOutOfService).CountAsync();

            var result = new GetTruckAvailabilityDataOutput
            {
                Available = availableTrucks,
                OutOfService = outOfServiceTrucks
            };

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_DriverDotRequirements)]
        public async Task<GetTenantDashboardStatusDataOutput> GetTruckServiceStatusData()
        {
            var today = await GetToday();
            var query = _truckRepository.GetAll().Where(t => t.IsActive && t.LocationId != null && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true);

            var noService = await query.Where(t => !t.PreventiveMaintenances.Any()).CountAsync();
            var overdueForService = await query.Where(t => t.PreventiveMaintenances.Any(f => (f.DueDate.HasValue && f.DueDate < today) ||
                                                                                             (f.DueMileage.HasValue && f.DueMileage < t.CurrentMileage) ||
                                                                                             (f.DueHour.HasValue && f.DueHour < t.CurrentHours))).CountAsync();

            var dueForService = await query.Where(t => t.PreventiveMaintenances.Any(f => (f.WarningDate.HasValue && f.WarningDate < today) ||
                                                                                            (f.WarningMileage.HasValue && f.WarningMileage < t.CurrentMileage) ||
                                                                                            (f.WarningHour.HasValue && f.WarningHour < t.CurrentHours)))
                                            .Where(t => !t.PreventiveMaintenances.Any(f => (f.DueDate.HasValue && f.DueDate < today) ||
                                                                                             (f.DueMileage.HasValue && f.DueMileage < t.CurrentMileage) ||
                                                                                             (f.DueHour.HasValue && f.DueHour < t.CurrentHours))).CountAsync();

            var withService = await query.Where(t => t.PreventiveMaintenances.Any())
                                        .Where(t => !t.PreventiveMaintenances.Any(f => (f.WarningDate.HasValue && f.WarningDate < today) ||
                                                                                            (f.WarningMileage.HasValue && f.WarningMileage < t.CurrentMileage) ||
                                                                                            (f.WarningHour.HasValue && f.WarningHour < t.CurrentHours)))
                                        .Where(t => !t.PreventiveMaintenances.Any(f => (f.DueDate.HasValue && f.DueDate < today) ||
                                                                                             (f.DueMileage.HasValue && f.DueMileage < t.CurrentMileage) ||
                                                                                             (f.DueHour.HasValue && f.DueHour < t.CurrentHours))).CountAsync();

            var result = new GetTenantDashboardStatusDataOutput
            {
                NoData = noService,
                Overdue = overdueForService,
                Due = dueForService,
                Ok = withService
            };

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_DriverDotRequirements)]
        public async Task<GetTenantDashboardStatusDataOutput> GetTruckLicensePlateStatusData()
        {
            var today = await GetToday();
            var query = _truckRepository.GetAll().Where(t => t.IsActive && t.LocationId != null && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true);

            var noData = await query.Where(t => t.PlateExpiration == null).CountAsync();
            var overdue = await query.Where(t => t.PlateExpiration < today).CountAsync();
            var due = await query.Where(t => t.PlateExpiration >= today && t.PlateExpiration <= today.AddDays(30)).CountAsync();
            var ok = await query.Where(t => t.PlateExpiration > today.AddDays(30)).CountAsync();

            var result = new GetTenantDashboardStatusDataOutput
            {
                NoData = noData,
                Overdue = overdue,
                Due = due,
                Ok = ok
            };

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_DriverDotRequirements)]
        public async Task<GetTenantDashboardStatusDataOutput> GetDriverLicenseStatusData()
        {
            var today = await GetToday();
            var query = _driverRepository.GetAll()
                .Where(d => !d.IsInactive && !d.IsExternal);

            var noData = await query.Where(d => d.LicenseExpirationDate == null).CountAsync();
            var overdue = await query.Where(d => d.LicenseExpirationDate < today).CountAsync();
            var due = await query.Where(d => d.LicenseExpirationDate >= today && d.LicenseExpirationDate <= today.AddDays(30)).CountAsync();
            var ok = await query.Where(d => d.LicenseExpirationDate > today.AddDays(30)).CountAsync();

            return new GetTenantDashboardStatusDataOutput
            {
                NoData = noData,
                Overdue = overdue,
                Due = due,
                Ok = ok
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_DriverDotRequirements)]
        public async Task<GetTenantDashboardStatusDataOutput> GetDriverPhysicalStatusData()
        {
            var today = await GetToday();
            var query = _driverRepository.GetAll()
                .Where(d => !d.IsInactive && !d.IsExternal);

            var noData = await query.Where(d => d.NextPhysicalDueDate == null).CountAsync();
            var overdue = await query.Where(d => d.NextPhysicalDueDate < today).CountAsync();
            var due = await query.Where(d => d.NextPhysicalDueDate >= today && d.NextPhysicalDueDate <= today.AddDays(30)).CountAsync();
            var ok = await query.Where(d => d.NextPhysicalDueDate > today.AddDays(30)).CountAsync();

            return new GetTenantDashboardStatusDataOutput
            {
                NoData = noData,
                Overdue = overdue,
                Due = due,
                Ok = ok
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_DriverDotRequirements)]
        public async Task<GetTenantDashboardStatusDataOutput> GetDriverMVRStatusData()
        {
            var today = await GetToday();
            var query = _driverRepository.GetAll()
                .Where(d => !d.IsInactive && !d.IsExternal);

            var noData = await query.Where(d => d.NextMvrDueDate == null).CountAsync();
            var overdue = await query.Where(d => d.NextMvrDueDate < today).CountAsync();
            var due = await query.Where(d => d.NextMvrDueDate >= today && d.NextMvrDueDate <= today.AddDays(30)).CountAsync();
            var ok = await query.Where(d => d.NextMvrDueDate > today.AddDays(30)).CountAsync();

            return new GetTenantDashboardStatusDataOutput
            {
                NoData = noData,
                Overdue = overdue,
                Due = due,
                Ok = ok
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_Revenue)]
        public async Task<GetRevenueGraphDataOutput> GetRevenuePerTruckByDateGraphData(GetRevenueByDateGraphDataInput input)
        {
            var revenueData = await GetRevenueByDateGraphData(input);

            var dailyHistory = await _tenantDailyHistoryRepository.GetAll()
                .Where(x => x.Date >= input.PeriodBegin && x.Date <= input.PeriodEnd)
                .Select(x => new
                {
                    x.Date,
                    x.ActiveTrucks
                }).ToListAsync();

            switch (input.DatePeriod)
            {
                case RevenueGraphDatePeriod.Daily:
                    foreach (var item in revenueData.RevenueGraphData)
                    {
                        var activeTrucks = dailyHistory.FirstOrDefault(x => item.PeriodStart == x.Date)?.ActiveTrucks;
                        item.DivideRevenueBy(activeTrucks);
                    }
                    break;

                case RevenueGraphDatePeriod.Weekly:
                case RevenueGraphDatePeriod.Monthly:
                    foreach (var item in revenueData.RevenueGraphData)
                    {
                        var periodHistory = dailyHistory.Where(x => x.Date >= item.PeriodStart && x.Date <= item.PeriodEnd).ToList();
                        var activeTrucks = periodHistory.Any() ? periodHistory.Average(x => Convert.ToDecimal(x.ActiveTrucks)) : 0;
                        item.DivideRevenueBy(activeTrucks);
                    }
                    break;
            }

            return revenueData;
        }

        [AbpAuthorize(AppPermissions.Pages_Dashboard_Revenue)]
        public async Task<RevenueChartsDataDto> GetRevenueChartsData(GetRevenueChartsDataInput input)
        {
            var timezone = await GetTimezone();
            var periodBeginUtc = input.PeriodBegin.ConvertTimeZoneFrom(timezone);
            var periodEndUtc = input.PeriodEnd.AddDays(1).ConvertTimeZoneFrom(timezone);

            var result = new RevenueChartsDataDto
            {
                RequestedTicketType = input.TicketType,
                IsGPSConfigured = await _truckTelematicsAppService.IsGpsIntegrationConfigured()
            };

            if (input.TicketType == TicketType.Both || input.TicketType == TicketType.InternalTrucks)
            {
                result.FuelCostValue = await _fuelPurchaseRepository.GetAll()
                    .Where(x => x.FuelDateTime >= periodBeginUtc && x.FuelDateTime < periodEndUtc && x.Amount != null && x.Rate != null)
                    .Select(x => (x.Amount ?? 0) * (x.Rate ?? 0))
                    .SumAsync();
            }


            var revenueData = await GetRevenueByDateGraphData(new GetRevenueByDateGraphDataInput
            {
                DatePeriod = RevenueGraphDatePeriod.Daily,
                PeriodBegin = input.PeriodBegin,
                PeriodEnd = input.PeriodEnd,
                TicketType = input.TicketType
            });

            result.FreightRevenueValue = revenueData.RevenueGraphData.Sum(x => x.FreightRevenueValue);
            result.MaterialRevenueValue = revenueData.RevenueGraphData.Sum(x => x.MaterialRevenueValue);
            result.FuelSurchargeValue = revenueData.RevenueGraphData.Sum(x => x.FuelSurchargeValue);
            result.InternalTrucksFuelSurchargeValue = revenueData.RevenueGraphData.Sum(x => x.InternalTrucksFuelSurchargeValue);
            result.LeaseHaulersFuelSurchargeValue = revenueData.RevenueGraphData.Sum(x => x.LeaseHaulersFuelSurchargeValue);


            if (result.IsGPSConfigured && result.RequestedTicketType == TicketType.InternalTrucks)
            {
                var totalEngineHours = await GetVehicleUsageForPeriod(ReadingType.Hours, input.PeriodBegin, input.PeriodEnd);
                if (totalEngineHours > 0)
                {
                    result.AvgRevenuePerHourValue = result.TotalRevenue / totalEngineHours;
                }

                var totalMiles = await GetVehicleUsageForPeriod(ReadingType.Miles, input.PeriodBegin, input.PeriodEnd);
                if (totalMiles > 0)
                {
                    result.AvgRevenuePerMileValue = result.TotalRevenue / totalMiles;
                    result.AvgFuelCostPerMileValue = result.FuelCostValue / totalMiles;
                }
            }


            var averageActiveTruckCount = await _tenantDailyHistoryRepository.GetAll()
                .Where(x => x.Date >= input.PeriodBegin && x.Date <= input.PeriodEnd && x.ActiveTrucks > 0)
                .AverageAsync(x => (decimal?)x.ActiveTrucks);

            if (averageActiveTruckCount != 0)
            {
                result.FreightRevenuePerTruckValue = (result.FreightRevenueValue / averageActiveTruckCount) ?? 0;
                result.MaterialRevenuePerTruckValue = (result.MaterialRevenueValue / averageActiveTruckCount) ?? 0;
                result.FuelSurchargePerTruckValue = (result.FuelSurchargeValue / averageActiveTruckCount) ?? 0;
            }

            result.ProductionPayValue = await
                (from ticket in _ticketRepository.GetAll()
                    .Where(x => x.OrderLine.ProductionPay)
                    .Where(x => x.TicketDateTime >= periodBeginUtc && x.TicketDateTime < periodEndUtc)
                    .WhereIf(input.TicketType == TicketType.InternalTrucks, x => x.Driver.OfficeId != null)
                    .WhereIf(input.TicketType == TicketType.LeaseHaulers, x => x.Driver.OfficeId == null)
                 join productionPay in _employeeTimeClassificationRepository.GetAll() on ticket.DriverId equals productionPay.DriverId
                 select new
                 {
                     Ticket = ticket,
                     ProductionPay = productionPay
                 })
                .Where(x => x.ProductionPay.TimeClassification.IsProductionBased)
                .Select(x => new
                {
                    Quantity = x.Ticket.Quantity,
                    FreightRate = x.Ticket.OrderLine.FreightPricePerUnit ?? 0,
                    DriverPayRate = x.ProductionPay.PayRate,
                })
                .SumAsync(x => Math.Round(x.Quantity * x.FreightRate * x.DriverPayRate / 100, 2));

            result.HourlyPayValue = await
                (from employeeTime in _employeeTimeRepository.GetAll()
                    .Where(x => x.EndDateTime >= periodBeginUtc && x.EndDateTime < periodEndUtc)
                    .WhereIf(input.TicketType == TicketType.InternalTrucks, x => x.User.Drivers.Any(d => d.OfficeId != null))
                    .WhereIf(input.TicketType == TicketType.LeaseHaulers, x => x.User.Drivers.Any(d => d.OfficeId == null))
                 join timeClassification in _employeeTimeClassificationRepository.GetAll()
                     on new { DriverId = employeeTime.DriverId.Value, TimeClassificationId = employeeTime.TimeClassificationId }
                     equals new { timeClassification.DriverId, timeClassification.TimeClassificationId }
                 select new
                 {
                     EmployeeTime = employeeTime,
                     TimeClassification = timeClassification,
                 })
                .Where(x => x.EmployeeTime.EndDateTime != null && x.TimeClassification != null
                    && !x.TimeClassification.TimeClassification.IsProductionBased)
                .Select(x => new
                {
                    HoursAmount = (decimal)EF.Functions.DateDiffMinute(x.EmployeeTime.StartDateTime, x.EmployeeTime.EndDateTime.Value) / 60,
                    DriverPayRate = x.TimeClassification.PayRate
                })
                .SumAsync(x => x.HoursAmount * x.DriverPayRate);


            //if (input.TicketType == TicketType.Both || input.TicketType == TicketType.LeaseHaulers)
            //{
            result.LeaseHaulerPaymentValue = await _ticketRepository.GetAll()
                .Where(x => x.TicketDateTime >= periodBeginUtc && x.TicketDateTime < periodEndUtc)
                .Where(x => x.CarrierId.HasValue)
                .Select(x => new
                {
                    Rate = x.OrderLine.LeaseHaulerRate ?? 0,
                    Quantity = x.Quantity
                })
                .Where(x => x.Rate > 0)
                .SumAsync(x => x.Rate * x.Quantity);
            //}


            if (averageActiveTruckCount != 0)
            {
                result.AvgAdjustedRevnuePerTruckValue = (result.AdjustedRevenueValue / averageActiveTruckCount) ?? 0;
            }


            return result;
        }

        private async Task<decimal> GetVehicleUsageForPeriod(ReadingType readingType, DateTime startDateTime, DateTime endDateTime)
        {
            var timezone = await GetTimezone();
            var startDateTimeUtc = startDateTime.ConvertTimeZoneFrom(timezone);
            var endDateTimeUtc = endDateTime.AddDays(1).ConvertTimeZoneFrom(timezone);
            var previousData = _vehicleUsageRepository.GetAll()
                .Where(x => x.ReadingType == readingType
                    && x.ReadingDateTime <= startDateTimeUtc
                    && x.ReadingDateTime > startDateTimeUtc.AddDays(-1))
                //.OrderByDescending(x => x.ReadingDateTime)
                .GroupBy(x => x.TruckId)
                .Select(x => new
                {
                    TruckId = x.Key,
                    Reading = x.OrderByDescending(x => x.ReadingDateTime).First().Reading
                });

            var latestData = _vehicleUsageRepository.GetAll()
                .Where(x => x.ReadingType == readingType
                    && x.ReadingDateTime >= startDateTimeUtc
                    && x.ReadingDateTime < endDateTimeUtc)
                //.OrderByDescending(x => x.ReadingDateTime)
                .GroupBy(x => x.TruckId)
                .Select(x => new
                {
                    TruckId = x.Key,
                    Reading = x.OrderByDescending(x => x.ReadingDateTime).First().Reading
                });

            //could not be translated to a single query
            //return await (from previous in previousData
            //              join latest in latestData on previous.TruckId equals latest.TruckId
            //              where latest != null && previous != null
            //              select latest.Reading - previous.Reading).SumAsync();

            return (from previous in await previousData.ToListAsync()
                    join latest in await latestData.ToListAsync()
                    on previous.TruckId equals latest.TruckId
                    where latest != null && previous != null
                    select latest.Reading - previous.Reading).Sum();
        }

        public async Task<List<DashboardSettingDto>> GetDashboardSettings()
        {
            var userSettings = await _dashboardSettingManager.GetDashboardUserSettingsAsync();
            return userSettings.Select(x => new DashboardSettingDto
            {
                SettingName = x.SettingName,
                SettingLabel = L(x.SettingLocalizableLabel),
                IsEnabled = x.IsEnabled,
            }).ToList();
        }

        public async Task SaveDashboardSettings(List<string> checkedSettings)
        {
            var userSettings = await _dashboardSettingManager.GetDashboardUserSettingsAsync();
            foreach (var setting in userSettings)
            {
                await _dashboardSettingManager.SetDashboardUserSettingAsync(setting.SettingName, checkedSettings.Contains(setting.SettingName));
            }
        }

        public async Task<List<TenantDto>> GetTenants()
        {
            var tenants = await TenantManager.Tenants
                            .Where(t => t.IsActive == true && !t.IsDeleted)
                            .Select(t => new TenantDto
                            {
                                TenantId = t.Id,
                                TenantName = t.Name
                            })
                            .ToListAsync();
            return tenants;
        }

        public async Task<List<TenantDailyHistorySummary>> GetTenantStatistics(GetTenantStatisticsInput input)
        {
            var summary = new List<TenantDailyHistorySummary> {
                new TenantDailyHistorySummary() { 
                    TenantId = input.TenantId ?? 0
                }
            };

            try
            {
                var tenants = await TenantManager.Tenants
                            .Where(t => t.CreationTime >= input.StartDate &&
                                        t.CreationTime <= input.EndDate &&
                                        t.IsActive == true && !t.IsDeleted)
                            .ToListAsync();

                var tenantDailyHistoryData = await _tenantDailyHistoryRepository.GetAll()
                    .WhereIf(input.TenantId.HasValue, tdh => tdh.TenantId == input.TenantId.Value)
                    .Where(tdh => tdh.Date >= input.StartDate && tdh.Date <= input.EndDate)
                    .Select(s => new
                    {
                        s.TenantId,
                        TenantName = s.Tenant.Name,
                        s.Date.Year,
                        MonthName = s.Date.ToString("MMM"),
                        MonthNumber = s.Date.Month,
                        s.ActiveUsers,
                        s.InternalTrucksScheduled,
                        s.LeaseHaulerScheduledDeliveries,
                        s.OrderLinesCreated,
                        s.TicketsCreated,
                        s.SmsSent
                    })
                    .ToListAsync();

                summary = tenantDailyHistoryData
                                .GroupBy(p => new { p.TenantId, p.TenantName, p.Year, p.MonthName, p.MonthNumber })
                                .Select(g => new TenantDailyHistorySummary()
                                {
                                    TenantId = g.Key.TenantId,
                                    TenantName = g.Key.TenantName,
                                    MonthYear = g.Key.Year,
                                    MonthName = g.Key.MonthName,
                                    MonthNumber = g.Key.MonthNumber,
                                    ActiveUsers = g.Sum(a => a.ActiveUsers),
                                    TrucksScheduled = g.Sum(a => a.InternalTrucksScheduled) + g.Sum(a => a.LeaseHaulerScheduledDeliveries),
                                    OrderLines = g.Sum(a => a.OrderLinesCreated),
                                    Tickets = g.Sum(a => a.TicketsCreated),
                                    Sms = g.Sum(a => a.SmsSent),
                                    TenantsAdded = tenants.Where(p => p.CreationTime.Month == g.Key.MonthNumber && p.CreationTime.Year == g.Key.Year).Count()
                                })
                                .ToList();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
