using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.Tickets;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DispatcherWeb.Scheduling.Dto.OrderTrucksDto;

namespace DispatcherWeb.Scheduling
{

    [AbpAuthorize(AppPermissions.Pages_Schedule)]
    public class SchedulingAppService : DispatcherWebAppServiceBase, ISchedulingAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly IReadonlyCheckerFactory<OrderLine> _orderLineReadonlyCheckerFactory;
        private readonly ISmsSender _smsSender;
        private readonly IDispatchingAppService _dispatchingAppService;
        private readonly IOrderLineScheduledTrucksUpdater _orderLineScheduledTrucksUpdater;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly ITelematics _telematics;

        public SchedulingAppService(
            IRepository<Truck> truckRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IRepository<Driver> driverRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            OrderTaxCalculator orderTaxCalculator,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            IReadonlyCheckerFactory<OrderLine> orderLineReadonlyCheckerFactory,
            ISmsSender smsSender,
            IDispatchingAppService dispatchingAppService,
            IOrderLineScheduledTrucksUpdater orderLineScheduledTrucksUpdater,
            ICrossTenantOrderSender crossTenantOrderSender,
            ISyncRequestSender syncRequestSender,
            IDriverApplicationPushSender driverApplicationPushSender,
            IDriverApplicationLogger driverApplicationLogger,
            ITelematics telematics
        )
        {
            _truckRepository = truckRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _ticketRepository = ticketRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _dispatchRepository = dispatchRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
            _driverRepository = driverRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _orderTaxCalculator = orderTaxCalculator;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _orderLineReadonlyCheckerFactory = orderLineReadonlyCheckerFactory;
            _smsSender = smsSender;
            _dispatchingAppService = dispatchingAppService;
            _orderLineScheduledTrucksUpdater = orderLineScheduledTrucksUpdater;
            _crossTenantOrderSender = crossTenantOrderSender;
            _syncRequestSender = syncRequestSender;
            _driverApplicationPushSender = driverApplicationPushSender;
            _driverApplicationLogger = driverApplicationLogger;
            _telematics = telematics;
        }

        //truck tiles
        public async Task<ListResultDto<ScheduleTruckDto>> GetScheduleTrucks(GetScheduleTrucksInput input)
        {
            var showTrailersOnSchedule = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule);
            var trucksLite = await _truckRepository.GetAll()
                .WhereIf(!showTrailersOnSchedule, t => t.VehicleCategory.IsPowered)
                .GetScheduleTrucks(input, await SettingManager.UseShifts(),
                    await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature));

            if (await IsDateInThePast(input.Date))
            {
                trucksLite.ForEach(t => t.DefaultDriverId = null);
            }

            var trucks = await trucksLite
                .PopulateScheduleTruckFullFields(input, _driverAssignmentRepository.GetAll());

            return new ListResultDto<ScheduleTruckDto>(trucks);
        }
        private async Task<bool> IsDateInThePast(DateTime date)
        {
            return date < await GetToday();
        }

        //'add truck' dropdown on 'add truck' modal
        public async Task<ListResultDto<ScheduleOrderLineTruckDto>> GetTrucksForOrderLine(GetTrucksForOrderLineInput input)
        {
            var trucks = await _truckRepository.GetAll()
                .Where(t => t.IsActive && !t.IsOutOfService)
                .WhereIf(!input.TruckCode.IsNullOrEmpty(), x => x.TruckCode.StartsWith(input.TruckCode))
                .WhereIf(input.OnlyTrailers, x => x.VehicleCategory.AssetType == AssetType.Trailer)
                .WhereIf(input.IsPowered.HasValue, x => x.VehicleCategory.IsPowered == input.IsPowered.Value)
                .Where(x => x.OrderLineTrucks.All(olt => olt.OrderLine.Id != input.OrderLineId))
                .GetScheduleTrucks(input, await SettingManager.UseShifts(),
                    await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature));

            trucks.RemoveAll(x => x.Utilization >= 1 && x.VehicleCategory.IsPowered);

            var result = trucks.Select(x => new ScheduleOrderLineTruckDto
            {
                Id = 0,
                ParentId = null,
                OrderLineId = input.OrderLineId,
                TruckId = x.Id,
                TruckCode = x.TruckCode,
                OfficeId = x.OfficeId,
                IsExternal = x.IsExternal,
                VehicleCategory = new VehicleCategoryDto
                {
                    Id = x.VehicleCategory.Id,
                    AssetType = x.VehicleCategory.AssetType,
                    IsPowered = x.VehicleCategory.IsPowered,
                    Name = x.VehicleCategory.Name,
                    SortOrder = x.VehicleCategory.SortOrder
                },
                AlwaysShowOnSchedule = x.AlwaysShowOnSchedule,
                CanPullTrailer = x.CanPullTrailer,
                Utilization = x.Utilization
            }).ToList();

            return new ListResultDto<ScheduleOrderLineTruckDto>(result);
        }

        [HttpPost]
        public async Task<PagedResultDto<TruckToAssignDto>> GetTrucksToAssign(GetTrucksToAssignInput input)
        {
            var trucks = await (await _truckRepository.GetAll()
                .Where(t => t.IsActive && !t.IsOutOfService)
                .Where(t => t.VehicleCategory.IsPowered)
                .WhereIf(input.VehicleCategoryIds?.Any() == true, t => input.VehicleCategoryIds.Contains(t.VehicleCategoryId))
                .WhereIf(input.BedConstruction.HasValue, t => t.BedConstruction == input.BedConstruction)
                .WhereIf(input.IsApportioned.HasValue, t => t.IsApportioned == input.IsApportioned)
                .GetScheduleTrucks(input, await SettingManager.UseShifts(),
                    await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature)))
                .PopulateScheduleTruckFullFields(input, _driverAssignmentRepository.GetAll());

            if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization))
            {
                trucks.RemoveAll(x => x.Utilization >= 1 && x.VehicleCategory.IsPowered);
            }

            trucks.RemoveAll(x => x.HasNoDriver || !x.HasDefaultDriver && !x.HasDriverAssignment && (!x.IsExternal || x.DriverId == null));

            var assignedTruckIds = await _orderLineTruckRepository.GetAll()
                .Where(x => x.OrderLineId == input.OrderLineId)
                .Select(x => x.TruckId)
                .Distinct()
                .ToListAsync();
            trucks.RemoveAll(x => assignedTruckIds.Contains(x.Id));

            var items = trucks.Select(x => new TruckToAssignDto
            {
                Id = x.Id,
                TruckCode = x.TruckCode,
                BedConstruction = x.BedConstruction,
                DriverName = x.DriverName,
                IsApportioned = x.IsApportioned,
            })
            .WhereIf(!string.IsNullOrEmpty(input.DriverName), t => t.DriverName.Contains(input.DriverName))
            .AsQueryable()
            .OrderBy(input.Sorting)
            .ToList();

            return new PagedResultDto<TruckToAssignDto>(
                trucks.Count,
                items);
        }

        private async Task<IQueryable<OrderLine>> GetScheduleQueryAsync(GetScheduleOrdersInput input)
        {
            return _orderLineRepository.GetAll()
                .Where(ol =>
                    ol.Order.DeliveryDate == input.Date &&
                    !ol.Order.IsPending &&
                    (ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                )
                .WhereIf(await FeatureChecker.AllowMultiOfficeFeature(),
                    ol => ol.Order.LocationId == input.OfficeId || ol.SharedOrderLines.Any(sol => sol.OfficeId == input.OfficeId))
                .WhereIf(await SettingManager.UseShifts(), ol => ol.Order.Shift == input.Shift)
                .WhereIf(input.HideCompletedOrders, ol => !ol.IsComplete);
        }

        public async Task<PagedResultDto<ScheduleOrderLineDto>> GetScheduleOrders(GetScheduleOrdersInput input)
        {
            var query = await GetScheduleQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .GetScheduleOrders()
                .OrderBy(input.Sorting)
                .ToListAsync();

            await ConvertScheduleOrderTimesFromUtcAsync(items);

            var today = await GetToday();
            if (!input.HideProgressBar && input.Date == today)
            {
                await CalculateOrderLineProgress(items);
            }

            return new PagedResultDto<ScheduleOrderLineDto>(
                totalCount,
                items);
        }

        private async Task ConvertScheduleOrderTimesFromUtcAsync(List<ScheduleOrderLineDto> orderLines)
        {
            var timezone = await GetTimezone();
            orderLines.ForEach(x => ConvertScheduleOrderTimesFromUtc(x, timezone));
        }

        private void ConvertScheduleOrderTimesFromUtc(ScheduleOrderLineDto orderLine, string timezone)
        {
            orderLine.Time = orderLine.Time?.ConvertTimeZoneTo(timezone);
            orderLine.FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone);
            orderLine.Trucks.ForEach(x => ConvertScheduleOrderLineTruckTimesFromUtc(x, timezone));
        }

        private void ConvertScheduleOrderLineTruckTimesFromUtc(ScheduleOrderLineTruckDto truck, string timezone)
        {
            truck.TimeOnJob = truck.TimeOnJob?.ConvertTimeZoneTo(timezone);
        }

        private async Task CalculateOrderLineProgress(List<ScheduleOrderLineDto> items)
        {
            var dispatchVia = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia);

            if (dispatchVia != DispatchVia.DriverApplication)
            {
                return;
            }

            var orderLineIds = items.Select(x => x.Id).ToList();

            var progressData = await _orderLineRepository.GetAll()
                .Where(x => orderLineIds.Contains(x.Id))
                .Select(ol => new
                {
                    ol.Id,
                    //TicketLoads = ol.Tickets
                    Loads = ol.Dispatches.SelectMany(t => t.Loads).Select(l => new
                    {
                        l.DestinationDateTime,
                        l.SourceDateTime,
                        l.Dispatch.Acknowledged,
                        CargoCapacityTons = l.Dispatch.Truck.CargoCapacity,
                        l.Dispatch.Truck.CargoCapacityCyds,
                        l.Dispatch.TruckId,
                        l.Dispatch.Truck.TruckCode,
                        l.DispatchId,
                        Tickets = l.Tickets.Select(t => new TicketQuantityDto
                        {
                            Designation = t.OrderLine.Designation,
                            FreightUomId = t.OrderLine.FreightUomId,
                            MaterialUomId = t.OrderLine.MaterialUomId,
                            FuelSurcharge = t.FuelSurcharge,
                            Quantity = t.Quantity,
                            TicketUomId = t.UnitOfMeasureId
                        }).ToList()
                    }).ToList(),
                    //DeliveredLoads_FromOrder = ol.Dispatches.Sum(d => d.Loads.Count(l => l.DestinationDateTime.HasValue)),
                }).ToListAsync();

            foreach (var orderLine in items)
            {
                var designationHasMaterial = orderLine.Designation.HasMaterial();
                orderLine.AmountOrdered = designationHasMaterial ? orderLine.MaterialQuantity : orderLine.FreightQuantity;

                var orderLineProgress = progressData.FirstOrDefault(x => x.Id == orderLine.Id);

                if (orderLineProgress?.Loads.Any() == true)
                {
                    var deliveredLoads = orderLineProgress.Loads.Where(l => l.DestinationDateTime.HasValue).ToList();
                    var loadedLoads = orderLineProgress.Loads.Where(l => l.SourceDateTime.HasValue).ToList();
                    orderLine.DeliveredLoadCount = deliveredLoads.Count;
                    orderLine.LoadedLoadCount = loadedLoads.Count;
                    orderLine.LoadCount = orderLineProgress.Loads.Count;
                    orderLine.AmountLoaded = 0;
                    orderLine.AmountDelivered = 0;

                    if (!designationHasMaterial && orderLine.FreightUom?.ToLower().TrimEnd('s') == "hour")
                    {
                        continue;
                    }

                    //orderLine.HoursOnDispatches = 0;
                    //foreach (var loads in deliveredLoads
                    //    .Where(x => x.Acknowledged.HasValue)
                    //    .GroupBy(x => x.DispatchId))
                    //{
                    //    var load = loads.OrderByDescending(x => x.DestinationDateTime).FirstOrDefault();
                    //    if (load != null)
                    //    {
                    //        orderLine.HoursOnDispatches += (decimal)(load.DestinationDateTime.Value - load.Acknowledged.Value).TotalHours;
                    //    }
                    //}
                    //
                    //orderLine.HoursOnDispatchesLoaded = 0;
                    //foreach (var loads in loadedLoads
                    //    .Where(x => x.Acknowledged.HasValue)
                    //    .GroupBy(x => x.DispatchId))
                    //{
                    //    var load = loads
                    //        .OrderByDescending(x => x.DestinationDateTime != null)
                    //        .ThenByDescending(x => x.DestinationDateTime)
                    //        .ThenByDescending(x => x.SourceDateTime)
                    //        .FirstOrDefault();
                    //    if (load != null)
                    //    {
                    //        orderLine.HoursOnDispatchesLoaded += load.DestinationDateTime.HasValue
                    //            ? (decimal)(load.DestinationDateTime.Value - load.Acknowledged.Value).TotalHours
                    //            : (decimal)(load.SourceDateTime.Value - load.Acknowledged.Value).TotalHours;
                    //    }
                    //}

                    var validateCargoCapacityTons = false;
                    var validateCargoCapacityCyds = false;

                    foreach (var load in orderLineProgress.Loads)
                    {
                        var ticket = load.Tickets
                            .FirstOrDefault(t => designationHasMaterial
                                ? t.TicketUomId == t.MaterialUomId
                                : t.TicketUomId == t.FreightUomId);

                        decimal amountToAdd = 0M;

                        if (ticket != null)
                        {
                            amountToAdd = ticket.Quantity;
                        }
                        else
                        {
                            switch (designationHasMaterial
                                ? orderLine.MaterialUom?.ToLower()
                                : orderLine.FreightUom?.ToLower())
                            {
                                case "ton":
                                case "tons":
                                    validateCargoCapacityTons = true;
                                    amountToAdd = load.CargoCapacityTons ?? 0;
                                    break;
                                case "tonne":
                                case "tonnes":
                                    //% complete should be calculated by multiplying the number of loads by the CargoCapacity 
                                    //and any other conversion factor needed to get to the appropriate weight UOM and dividing by the ordered freight quantity
                                    validateCargoCapacityTons = true;
                                    amountToAdd = (load.CargoCapacityTons ?? 0) * 2000 / 2204.6M;
                                    break;
                                case "cubic yard":
                                case "cubic yards":
                                    //% complete should be calculated by multiplying the number of loads by the CargoCapacityCyds 
                                    //and dividing by the ordered freight quantity.
                                    validateCargoCapacityCyds = true;
                                    amountToAdd = load.CargoCapacityCyds ?? 0;
                                    break;
                                case "cubic meter":
                                case "cubic meters":
                                    validateCargoCapacityCyds = true;
                                    amountToAdd = (load.CargoCapacityCyds ?? 0) / 1.30795M;
                                    break;
                            }
                        }

                        if (load.SourceDateTime.HasValue)
                        {
                            orderLine.AmountLoaded += amountToAdd;
                        }
                        if (load.DestinationDateTime.HasValue)
                        {
                            orderLine.AmountDelivered += amountToAdd;
                        }
                    }

                    if (validateCargoCapacityTons)
                    {
                        orderLine.CargoCapacityRequiredError = ValidateCargoCapacityTons();
                    }

                    if (validateCargoCapacityCyds)
                    {
                        orderLine.CargoCapacityRequiredError = ValidateCargoCapacityCyds();
                    }
                }

                string ValidateCargoCapacityTons()
                {
                    var trucksWithNoCargoCapacity = orderLineProgress.Loads
                        .Where(x => x.DestinationDateTime.HasValue || x.SourceDateTime.HasValue)
                        .Where(x => !(x.CargoCapacityTons > 0))
                        .GroupBy(x => x.TruckId).Select(x => x.FirstOrDefault()?.TruckCode).ToList();

                    return FormatCargoCapacityError(trucksWithNoCargoCapacity, "Ave Load(Tons)");
                }

                string ValidateCargoCapacityCyds()
                {
                    var trucksWithNoCargoCapacity = orderLineProgress.Loads
                        .Where(x => x.DestinationDateTime.HasValue || x.SourceDateTime.HasValue)
                        .Where(x => !(x.CargoCapacityCyds > 0))
                        .GroupBy(x => x.TruckId).Select(x => x.FirstOrDefault()?.TruckCode).ToList();

                    return FormatCargoCapacityError(trucksWithNoCargoCapacity, "Ave Load(cyds)");
                }

                string FormatCargoCapacityError(List<string> truckNumbers, string fieldDisplayName)
                {
                    if (!truckNumbers.Any())
                    {
                        return null;
                    }

                    var s = truckNumbers.Count > 1 ? "s" : "";
                    return $"Can't calculate the estimated percentage because the value '{fieldDisplayName}' is not entered for truck{s} {string.Join(", ", truckNumbers)}";
                }
            }
        }

        public async Task<PagedResultDto<TruckOrderLineDto>> GetTruckOrderLinesPaged(GetTruckOrdersInput input)
        {
            var truckOrderLines = await GetTruckOrderLines(input);
            return new PagedResultDto<TruckOrderLineDto>(truckOrderLines.Count, truckOrderLines.ToList());
        }
        public async Task<IList<TruckOrderLineDto>> GetTruckOrderLines(GetTruckOrdersInput input)
        {
            var truckOrders = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLine.Order.DeliveryDate == input.ScheduleDate
                        && olt.OrderLine.Order.Shift == input.Shift
                        && olt.TruckId == input.TruckId)
                .Select(olt => new TruckOrderLineDto
                {
                    OrderLineId = olt.OrderLineId,
                    OrderId = olt.OrderLine.OrderId,
                    DriverId = olt.DriverId,
                    DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                    TruckStartTime = olt.TimeOnJob,
                    OrderLineStartTime = olt.OrderLine.TimeOnJob,
                    Customer = olt.OrderLine.Order.Customer.Name,
                    LoadAt = olt.OrderLine.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = olt.OrderLine.LoadAt.Name,
                        StreetAddress = olt.OrderLine.LoadAt.StreetAddress,
                        City = olt.OrderLine.LoadAt.City,
                        State = olt.OrderLine.LoadAt.State
                    },
                    DeliverTo = olt.OrderLine.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = olt.OrderLine.DeliverTo.Name,
                        StreetAddress = olt.OrderLine.DeliverTo.StreetAddress,
                        City = olt.OrderLine.DeliverTo.City,
                        State = olt.OrderLine.DeliverTo.State
                    },
                    Designation = olt.OrderLine.Designation,
                    Utilization = olt.Utilization,
                    Item = olt.OrderLine.Service.Service1,
                    MaterialUom = olt.OrderLine.MaterialUom.Name,
                    FreightUom = olt.OrderLine.FreightUom.Name,
                    MaterialQuantity = olt.OrderLine.MaterialQuantity,
                    FreightQuantity = olt.OrderLine.FreightQuantity,
                    SharedDateTime = olt.OrderLine.SharedDateTime,
                })
                .ToListAsync();

            truckOrders = truckOrders
                .GroupBy(x => new { x.OrderLineId, x.DriverId })
                .Select(g => new TruckOrderLineDto
                {
                    OrderLineId = g.Key.OrderLineId,
                    OrderId = g.First().OrderId,
                    DriverId = g.Key.DriverId,
                    DriverName = g.First().DriverName,
                    TruckStartTime = g.First().TruckStartTime,
                    OrderLineStartTime = g.First().OrderLineStartTime,
                    Customer = g.First().Customer,
                    LoadAt = g.First().LoadAt,
                    DeliverTo = g.First().DeliverTo,
                    Designation = g.First().Designation,
                    Utilization = g.Sum(x => x.Utilization),
                    Item = g.First().Item,
                    MaterialUom = g.First().MaterialUom,
                    FreightUom = g.First().FreightUom,
                    MaterialQuantity = g.First().MaterialQuantity,
                    FreightQuantity = g.First().FreightQuantity,
                    SharedDateTime = g.First().SharedDateTime,
                })
                .OrderBy(p => p.StartTime)
                .ToList();

            var timezone = await GetTimezone();
            foreach (var truckOrder in truckOrders)
            {
                truckOrder.TruckStartTime = truckOrder.TruckStartTime?.ConvertTimeZoneTo(timezone);
                truckOrder.OrderLineStartTime = truckOrder.OrderLineStartTime?.ConvertTimeZoneTo(timezone);
            }

            return truckOrders;
        }

        public async Task<DateTime?> GetStartTimeForTruckOrderLines(GetTruckOrdersInput input)
        {
            var result = await _driverAssignmentRepository.GetAll()
                .Where(da => da.Date == input.ScheduleDate && da.Shift == input.Shift && da.TruckId == input.TruckId)
                .Select(da => da.StartTime)
                .MinAsync();

            result = result?.ConvertTimeZoneTo(await GetTimezone());

            return result;
        }

        public async Task<IList<OrderLineTruckDto>> GetOrderLineTrucks(int orderLineId)
        {
            return await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == orderLineId)
                .SelectMany(ol => ol.OrderLineTrucks)
                .Select(olt => new OrderLineTruckDto
                {
                    TruckId = olt.TruckId,
                    TruckCode = olt.Truck.TruckCode,
                })
                .ToListAsync();
        }

        public async Task<AddOrderTruckResult> AddOrderLineTruck(AddOrderLineTruckInput input) =>
            await AddOrderLineTruckInternal(new AddOrderLineTruckInternalInput(input, 1));

        private async Task<AddOrderTruckResult> AddOrderLineTruckInternal(AddOrderLineTruckInternalInput input)
        {
            var scheduleOrderLine = await _orderLineRepository.GetAll()
                .Where(x => x.Id == input.OrderLineId)
                .GetScheduleOrders()
                .FirstAsync();

            ConvertScheduleOrderTimesFromUtc(scheduleOrderLine, await GetTimezone());

            var isOrderForPast = scheduleOrderLine.Date < await GetToday();

            var truck = (await _truckRepository.GetAll()
                .Where(x => x.Id == input.TruckId)
                .GetScheduleTrucks(new GetScheduleInput
                {
                    OfficeId = scheduleOrderLine.OfficeId,
                    Date = scheduleOrderLine.Date,
                    Shift = scheduleOrderLine.Shift,
                },
                await SettingManager.UseShifts(),
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature),
                skipTruckFiltering: true)).First();
            if (truck.VehicleCategory.AssetType == AssetType.Trailer)
            {
                truck.Utilization = 0;
            }

            var utilization = Math.Min(input.Utilization, await GetRemainingTruckUtilizationForOrderLineAsync(scheduleOrderLine, truck));
            if (utilization <= 0 && !isOrderForPast)
            {
                return new AddOrderTruckResult
                {
                    IsFailed = true,
                    ErrorMessage = "Truck or Order is fully utilized"
                };
            }

            if (scheduleOrderLine.Utilization > 0
                && (scheduleOrderLine.IsMaterialPriceOverridden || scheduleOrderLine.IsFreightPriceOverridden)
                && truck.VehicleCategory.AssetType != AssetType.Trailer)
            {
                return new AddOrderTruckResult
                {
                    IsFailed = true,
                    ErrorMessage = L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError")
                };
            }

            var orderLineTruck = new OrderLineTruck
            {
                OrderLineId = input.OrderLineId,
                TruckId = input.TruckId,
                DriverId = input.DriverId ?? truck.DriverId,
                ParentOrderLineTruckId = input.ParentId,
                Utilization = utilization,
                TimeOnJob = await GetTimeOnJobUtcForNewOrderLineTruck(input.OrderLineId, truck.VehicleCategory)
            };

            await _orderLineTruckRepository.InsertAsync(orderLineTruck);

            if (!isOrderForPast)
            {
                await CreateDriverAssignmentWhenAddingOrderLineTruck(scheduleOrderLine.Date, scheduleOrderLine.Shift, truck, scheduleOrderLine.OfficeId);
            }
            await SaveOrThrowConcurrencyErrorAsync();

            return new AddOrderTruckResult
            {
                Item = new ScheduleOrderLineTruckDto
                {
                    Id = orderLineTruck.Id,
                    OrderLineId = orderLineTruck.OrderLineId,
                    TruckId = orderLineTruck.TruckId,
                    OfficeId = truck.OfficeId,
                    IsExternal = truck.IsExternal,
                    ParentId = orderLineTruck.ParentOrderLineTruckId,
                    Utilization = orderLineTruck.Utilization,
                    VehicleCategory = new VehicleCategoryDto
                    {
                        Id = truck.VehicleCategory.Id,
                        Name = truck.VehicleCategory.Name,
                        AssetType = truck.VehicleCategory.AssetType,
                        IsPowered = truck.VehicleCategory.IsPowered,
                        SortOrder = truck.VehicleCategory.SortOrder
                    },
                    AlwaysShowOnSchedule = truck.AlwaysShowOnSchedule,
                    CanPullTrailer = truck.CanPullTrailer,
                    TruckCode = truck.TruckCode
                },
                OrderUtilization = scheduleOrderLine.Utilization + (truck.VehicleCategory.IsPowered ? utilization : 0)
            };
        }

        private async Task<DateTime?> GetTimeOnJobUtcForNewOrderLineTruck(int orderLineId, VehicleCategoryDto vehicleCategory)
        {
            var orderLine = await _orderLineRepository.GetAll()
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    x.StaggeredTimeKind,
                    x.StaggeredTimeInterval,
                    FirstStaggeredTimeOnJobUtc = x.FirstStaggeredTimeOnJob,
                    LastTruckTimeOnJobUtc = x.OrderLineTrucks
                        .Where(t => t.TimeOnJob != null && t.Truck.VehicleCategory.AssetType != AssetType.Trailer)
                        .OrderByDescending(t => t.Id)
                        .Select(t => t.TimeOnJob).FirstOrDefault()
                }).FirstAsync();

            if (orderLine.StaggeredTimeKind == StaggeredTimeKind.None)
            {
                return null;
            }

            if (orderLine.StaggeredTimeKind == StaggeredTimeKind.SetInterval)
            {
                var lastTimeOnJobUtc = orderLine.LastTruckTimeOnJobUtc;
                if (lastTimeOnJobUtc == null)
                {
                    return orderLine.FirstStaggeredTimeOnJobUtc;
                }
                if (vehicleCategory.AssetType == AssetType.Trailer)
                {
                    return lastTimeOnJobUtc;
                }
                return lastTimeOnJobUtc?.AddMinutes(orderLine.StaggeredTimeInterval ?? 0);
            }

            return null;
        }

        public async Task<int?> GetDefaultTrailerId(int truckId) =>
            await _truckRepository.GetAll().Where(t => t.Id == truckId).Select(t => t.DefaultTrailerId).FirstOrDefaultAsync();

        public async Task<decimal> GetTruckUtilization(GetTruckUtilizationInput input)
        {
            return await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.TruckId == input.TruckId &&
                              olt.OrderLine.Order.DeliveryDate == input.Date &&
                              olt.OrderLine.Order.Shift == input.Shift)
                .Select(olt => olt.Utilization)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsTrailerAssignedToAnotherTruck(IsTrailerAssignedToAnotherTruckInput input)
        {
            return await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.TruckId == input.TrailerId &&
                              olt.ParentOrderLineTruck.TruckId != input.ParentTruckId &&
                              olt.OrderLine.Order.DeliveryDate == input.Date &&
                              olt.OrderLine.Order.Shift == input.Shift &&
                              !olt.IsDone)
                .AnyAsync();
        }

        private async Task CreateDriverAssignmentWhenAddingOrderLineTruck(DateTime date, Shift? shift, ScheduleTruckDto truck, int officeId)
        {
            if (!truck.VehicleCategory.IsPowered ||
                !await FeatureChecker.AllowLeaseHaulersFeature() && truck.AlwaysShowOnSchedule)
            {
                return;
            }

            var truckId = truck.Id;

            if (await DriverAssignmentWithDriverExists())
            {
                return;
            }

            int? defaultDriverId = await GetDriverIdFromAvailableLeaseHaulerTruck() ?? await GetDefaultDriverId();
            if (defaultDriverId == null)
            {
                throw new UserFriendlyException("Cannot add OrderLineTruck for a truck without a default driver!");
            }
            await CreateDriverAssignment();

            // Local functions
            async Task<bool> DriverAssignmentWithDriverExists()
            {
                DriverAssignment driverAssignment = await _driverAssignmentRepository.GetAll()
                    .Where(da => da.TruckId == truckId && da.Date == date && da.Shift == shift)
                    .FirstOrDefaultAsync();
                if (driverAssignment != null)
                {
                    if (driverAssignment.DriverId == null)
                    {
                        throw new ApplicationException("Cannot add OrderLineTruck for a truck with no driver!");
                    }
                    return true;
                }
                return false;
            }

            async Task<int?> GetDriverIdFromAvailableLeaseHaulerTruck()
            {
                if (!await FeatureChecker.AllowLeaseHaulersFeature() || !truck.IsExternal)
                {
                    return null;
                }
                return (await _availableLeaseHaulerTruckRepository.GetAll()
                    .Where(alht => alht.TruckId == truckId && alht.Date == date && alht.Shift == shift)
                    .Select(alht => new { alht.DriverId })
                    .FirstOrDefaultAsync())?.DriverId;
            }

            async Task<int?> GetDefaultDriverId() => await _truckRepository.GetAll().Where(t => t.Id == truckId).Select(t => t.DefaultDriverId).FirstOrDefaultAsync();

            async Task CreateDriverAssignment()
            {
                var driverAssignment = new DriverAssignment
                {
                    Date = date,
                    Shift = shift,
                    DriverId = defaultDriverId,
                    OfficeId = officeId,
                    TruckId = truckId,
                };
                await _driverAssignmentRepository.InsertAsync(driverAssignment);

                await CurrentUnitOfWork.SaveChangesAsync();

                if (defaultDriverId.HasValue)
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(defaultDriverId.Value)
                    {
                        LogMessage = $"Created driver assignment when adding order line truck"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity())
                    .AddLogMessage("Created driver assignment when adding order line truck"));
            }
        }

        public async Task DeleteOrderLineTrucks(DeleteOrderLineTrucksInput input)
        {
            await _orderLineScheduledTrucksUpdater.DeleteOrderLineTrucks(input);
        }

        public async Task<DeleteOrderLineTruckResult> DeleteOrderLineTruck(DeleteOrderLineTruckInput input)
        {
            await ThrowIfTruckHasDispatches(input);

            await _dispatchingAppService.CancelOrEndAllDispatches(new CancelOrEndAllDispatchesInput
            {
                OrderLineId = input.OrderLineId,
                TruckId = (await _orderLineTruckRepository.GetAsync(input.OrderLineTruckId)).TruckId
            });

            if (input.MarkAsDone)
            {
                var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                    .Where(x => x.Id == input.OrderLineTruckId || x.ParentOrderLineTruckId == input.OrderLineTruckId)
                    .ToListAsync();
                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }
            }
            else
            {
                await _orderLineTruckRepository.DeleteAsync(x => x.Id == input.OrderLineTruckId || x.ParentOrderLineTruckId == input.OrderLineTruckId);
                await CurrentUnitOfWork.SaveChangesAsync();
                var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);
                var order = await orderLineUpdater.GetOrderAsync();
                if (order.DeliveryDate >= await GetToday())
                {
                    orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                    await orderLineUpdater.SaveChangesAsync();
                }
            }
            await SaveOrThrowConcurrencyErrorAsync();
            var orderLineUtilization = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == input.OrderLineId)
                .Select(ol => ol.OrderLineTrucks.Where(t => t.Truck.VehicleCategory.IsPowered).Sum(olt => olt.Utilization))
                .FirstAsync();
            return new DeleteOrderLineTruckResult
            {
                OrderLineUtilization = orderLineUtilization
            };
        }

        private async Task ThrowIfTruckHasDispatches(DeleteOrderLineTruckInput input)
        {
            var hasDispatches = await HasDispatches(input);
            if (hasDispatches.AcknowledgedOrLoaded)
            {
                throw new UserFriendlyException(L("TruckHasDispatch_YouMustCancelItFirstToRemoveTruck", hasDispatches.TruckCode));
            }
        }

        public async Task<MoveTruckResult> MoveTruck(MoveTruckInput input)
        {
            DateTime orderDate = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.Id == input.SourceOrderLineTruckId)
                .Select(ol => ol.OrderLine.Order.DeliveryDate.Value)
                .FirstAsync();
            DateTime today = await GetToday();
            if (orderDate < today)
            {
                throw new UserFriendlyException("You cannot move trucks for past orders");
            }
            bool markAsDone = today == orderDate;

            MoveTruckResult result = new MoveTruckResult();
            var sourceOrderLineTruck = await _orderLineTruckRepository.GetAsync(input.SourceOrderLineTruckId);
            await DeleteAndCreateOrReactivateOrderLineTruck();
            return result;

            // Local functions
            async Task DeleteAndCreateOrReactivateOrderLineTruck()
            {
                var existingDestinationOrderLineTruck = await _orderLineTruckRepository.GetAll()
                    .Where(olt => olt.OrderLineId == input.DestinationOrderLineId && olt.TruckId == sourceOrderLineTruck.TruckId)
                    .FirstOrDefaultAsync();
                if (existingDestinationOrderLineTruck != null && existingDestinationOrderLineTruck.IsDone)
                {
                    var utilization = sourceOrderLineTruck.Utilization;
                    await DeleteOrderLineTruck();
                    await ActivateClosedTruck();
                    await SetOrderLineTruckUtilization(existingDestinationOrderLineTruck.Id, utilization);
                }
                else if (existingDestinationOrderLineTruck == null)
                {
                    await DeleteOrderLineTruck();
                    await CreateOrderLineTruck();
                }
                else
                {
                    result.OrderLineTruckExists = true;
                }
            }

            async Task DeleteOrderLineTruck()
            {
                var deleteOrderLineTruckInput = new DeleteOrderLineTruckInput
                {
                    OrderLineId = sourceOrderLineTruck.OrderLineId,
                    OrderLineTruckId = sourceOrderLineTruck.Id,
                    MarkAsDone = markAsDone
                };
                await this.DeleteOrderLineTruck(deleteOrderLineTruckInput);
            }

            async Task ActivateClosedTruck()
            {
                var activateClosedTrucksInput = new ActivateClosedTrucksInput
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckIds = new[] { sourceOrderLineTruck.TruckId },
                };
                await ActivateClosedTrucks(activateClosedTrucksInput);
            }

            async Task SetOrderLineTruckUtilization(int orderLineTruckId, decimal utilization)
            {
                var orderLineTruckUtilizationEditDto = new OrderLineTruckUtilizationEditDto
                {
                    OrderLineId = input.DestinationOrderLineId,
                    OrderLineTruckId = orderLineTruckId,
                    Utilization = utilization,
                };
                await SetOrderTruckUtilization(orderLineTruckUtilizationEditDto);
            }

            async Task CreateOrderLineTruck()
            {
                var addOrderLineTruckInput = new AddOrderLineTruckInternalInput()
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckId = sourceOrderLineTruck.TruckId,
                    DriverId = sourceOrderLineTruck.DriverId,
                    ParentId = sourceOrderLineTruck.ParentOrderLineTruckId,
                    Utilization = !sourceOrderLineTruck.IsDone ? sourceOrderLineTruck.Utilization : 1,
                };
                var addOrderTruckResult = await AddOrderLineTruckInternal(addOrderLineTruckInput);
                if (addOrderTruckResult.IsFailed)
                {
                    throw new UserFriendlyException(addOrderTruckResult.ErrorMessage);
                }
            }

        }

        public async Task<HasDispatchesResult> HasDispatches(DeleteOrderLineTruckInput input)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.Id == input.OrderLineTruckId)
                .Select(olt => new
                {
                    olt.TruckId,
                    olt.Truck.TruckCode,
                    Dispatches = olt.OrderLine.Dispatches.Where(x => x.TruckId == olt.TruckId).Select(d => new
                    {
                        d.Status
                    }).ToList()
                }).FirstAsync();

            return new HasDispatchesResult
            {
                TruckCode = orderLineTruck.TruckCode,
                Unacknowledged = orderLineTruck.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Created, DispatchStatus.Sent)),
                AcknowledgedOrLoaded = orderLineTruck.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Loaded))
            };

            //await _orderLineTruckRepository.GetAll()
            //    .Where(olt => olt.Id == input.OrderLineTruckId)
            //    .Select(olt => new HasDispatchesResult
            //    {
            //        Unacknowledged = olt.OrderLine.Dispatches.Any(d => (d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Sent) && d.Truck.OrderLineTrucks.Any(dolt => dolt.Id == input.OrderLineTruckId)),
            //        AcknowledgedOrLoaded = olt.OrderLine.Dispatches.Any(d => (d.Status == DispatchStatus.Acknowledged || d.Status == DispatchStatus.Loaded) && d.Truck.OrderLineTrucks.Any(dolt => dolt.Id == input.OrderLineTruckId))
            //    })
            //    .FirstAsync();
        }

        public async Task<List<HasDispatchesResult>> OrderLineHasDispatches(DeleteOrderLineTruckInput input)
        {
            var existingDispatches = await _orderLineTruckRepository.GetAll()
                    .Where(olt => olt.OrderLineId == input.OrderLineId)
                    .Select(olt => new
                    {
                        olt.TruckId,
                        olt.Truck.TruckCode,
                        Dispatches = olt.OrderLine.Dispatches.Where(x => x.TruckId == olt.TruckId).Select(d => new
                        {
                            d.Status
                        }).ToList()
                    }).ToListAsync();

            return existingDispatches.Select(x => new HasDispatchesResult
            {
                TruckCode = x.TruckCode,
                Unacknowledged = x.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Created, DispatchStatus.Sent)),
                AcknowledgedOrLoaded = x.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Loaded))
            }).ToList();
        }

        public async Task<HasDispatchesResult> SomeOrderLineTrucksHaveDispatches(SomeOrderLineTrucksHaveDispatchesInput input)
        {
            var dispatchQuery = _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == input.OrderLineId && input.TruckIds.Contains(olt.TruckId))
                .SelectMany(olt => olt.OrderLine.Dispatches);
            return new HasDispatchesResult()
            {
                Unacknowledged = await dispatchQuery.AnyAsync(d => d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Sent),
                AcknowledgedOrLoaded = await dispatchQuery.AnyAsync(d => d.Status == DispatchStatus.Acknowledged || d.Status == DispatchStatus.Loaded),
            };
        }

        public async Task<bool> IsOrderLineFieldReadonly(IsOrderLineFieldReadonlyInput input)
        {
            var readonlyChecker = _orderLineReadonlyCheckerFactory.Create(input.OrderLineId);
            return await readonlyChecker.IsFieldReadonlyAsync(input.FieldName);
        }


        private class RemainingTruckUtilizationAndNumber
        {
            public decimal RemainingUtilization { get; set; }
            public int TruckNumber { get; set; }
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<CopyOrderTrucksResult> CopyOrdersTrucks(CopyOrdersTrucksInput input)
        {
            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = true }))
            {
                CopyOrderTrucksResult allResult = new CopyOrderTrucksResult()
                {
                    Completed = true,
                };

                foreach (var newOrderId in input.NewOrderIds)
                {
                    var result = await CopyOrderTrucksInternal(new CopyOrderTrucksInput()
                    {
                        NewOrderId = newOrderId,
                        OriginalOrderId = input.OriginalOrderId,
                        OrderLineId = input.OrderLineId,
                        ProceedOnConflict = input.ProceedOnConflict,
                    });
                    if (!result.Completed)
                    {
                        allResult.Completed = false;
                        allResult.ConflictingTrucks = allResult.ConflictingTrucks ?? new List<string>();
                        allResult.ConflictingTrucks.AddRange(result.ConflictingTrucks.Where(x => !allResult.ConflictingTrucks.Contains(x)));
                    }
                    allResult.SomeTrucksAreNotCopied = allResult.SomeTrucksAreNotCopied || result.SomeTrucksAreNotCopied;
                }

                if (allResult.Completed)
                {
                    await unitOfWork.CompleteAsync();
                }

                return allResult;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<CopyOrderTrucksResult> CopyOrderTrucks(CopyOrderTrucksInput input)
        {
            return await CopyOrderTrucksInternal(input);
        }

        private async Task<CopyOrderTrucksResult> CopyOrderTrucksInternal(CopyOrderTrucksInput input)
        {
            var result = new CopyOrderTrucksResult();
            var timezone = await GetTimezone();

            var newOrder = await _orderRepository.GetAll()
                .Where(x => x.Id == input.NewOrderId)
                .Select(x => new
                {
                    OfficeId = x.LocationId,
                    Date = x.DeliveryDate,
                    x.Shift
                })
                .FirstAsync();

            if (!newOrder.Date.HasValue)
            {
                result.Completed = false;
                return result;
            }

            var originalOrderLines = await _orderLineRepository.GetAll()
                .Where(ol => ol.OrderId == input.OriginalOrderId)
                .WhereIf(input.OrderLineId.HasValue, ol => ol.Id == input.OrderLineId.Value)
                .ToListAsync();

            var newOrderLines = await _orderLineRepository.GetAll()
                .Where(ol => ol.OrderId == input.NewOrderId)
                .ToListAsync();

            var originalOrderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Include(x => x.Truck)
                .AsNoTracking()
                .Where(x => x.OrderLine.OrderId == input.OriginalOrderId)
                .WhereIf(input.OrderLineId.HasValue, olt => olt.OrderLineId == input.OrderLineId.Value)
                .ToListAsync();

            var originalTrucksIds = originalOrderLineTrucks.Select(x => x.TruckId).ToList();

            var originalOrder = await _orderRepository.GetAll()
                .Where(o => o.Id == input.OriginalOrderId)
                .Select(o => new
                {
                    o.Id,
                    o.DeliveryDate,
                    o.Shift,
                })
                .FirstAsync();
            var originalDriverAssignments = await _driverAssignmentRepository.GetAll()
                .AsNoTracking()
                .Where(da =>
                    originalTrucksIds.Contains(da.TruckId) &&
                    da.Date == originalOrder.DeliveryDate &&
                    da.Shift == originalOrder.Shift)
                .ToListAsync();

            var trucks = (await _truckRepository.GetAll()
                .Where(t => originalTrucksIds.Contains(t.Id))
                .GetScheduleTrucks(new GetScheduleInput
                {
                    OfficeId = newOrder.OfficeId,
                    Date = newOrder.Date.Value,
                    Shift = newOrder.Shift,
                },
                await SettingManager.UseShifts(),
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature),
                skipTruckFiltering: false)).ToList();

            var truckInfos = await _truckRepository.GetAll()
                .Where(t => originalTrucksIds.Contains(t.Id))
                .Select(t => new
                {
                    t.Id,
                    t.DefaultDriverId,
                    t.LocationId
                }).ToListAsync();

            var existingDriverAssignments = await _driverAssignmentRepository.GetAll()
                .Where(x => x.Date == newOrder.Date && x.Shift == newOrder.Shift)
                .ToListAsync();

            var passedOrderLineTrucks = new List<OrderLineTruck>();

            var truckRemainingUtilizationQuery =
                from olt in originalOrderLineTrucks
                group olt by olt.TruckId
                into truckGroup
                select new
                {
                    TruckId = truckGroup.Key,
                    RemainingUtilization = 1 - truckGroup.Sum(x => x.Utilization),
                    TruckNumber = truckGroup.Count(),
                };
            var truckRemainingUtilizationDictionary = truckRemainingUtilizationQuery.ToDictionary(x => x.TruckId, x => new RemainingTruckUtilizationAndNumber { RemainingUtilization = x.RemainingUtilization, TruckNumber = x.TruckNumber });

            foreach (var originalOrderLineTruck in originalOrderLineTrucks.ToList())
            {
                var truck = trucks.FirstOrDefault(x => x.Id == originalOrderLineTruck.TruckId);
                var truckInfo = truckInfos.FirstOrDefault(x => x.Id == originalOrderLineTruck.TruckId);
                if (truck == null || truck.IsOutOfService || !truck.IsActive || truckInfo == null)
                {
                    originalOrderLineTrucks.Remove(originalOrderLineTruck);
                    continue;
                }

                if (await AllowLeaseHaulerAndTruckIsLeaseHauler(truck))
                {
                    originalOrderLineTrucks.Remove(originalOrderLineTruck);
                    continue;
                }

                int? newDriverId = null;
                if (truck.VehicleCategory.AssetType != AssetType.Trailer && truck.VehicleCategory.IsPowered)
                {
                    var existingDriverAssignment = existingDriverAssignments.FirstOrDefault(da => da.TruckId == truck.Id);
                    if (existingDriverAssignment != null)
                    {
                        newDriverId = existingDriverAssignment.DriverId;
                    }
                    else
                    {
                        newDriverId = truckInfo.DefaultDriverId;
                        DriverAssignment newDriverAssignment = new DriverAssignment
                        {
                            Date = newOrder.Date.Value,
                            Shift = newOrder.Shift,
                            DriverId = truckInfo.DefaultDriverId,
                            TruckId = truck.Id,
                            //StartTime = originalDriverAssignment.StartTime.HasValue
                            //        ? newOrder.Date.Date.Add(originalDriverAssignment.StartTime.Value.TimeOfDay)
                            //        : (DateTime?)null,
                            OfficeId = truckInfo.LocationId, //originalDriverAssignment.OfficeId,
                            //Note = originalDriverAssignment.Note,
                        };
                        existingDriverAssignments.Add(newDriverAssignment);
                        await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                    }
                }

                int newOrderLineId = MapOriginalOrderLineIdToNewOrderLineId(originalOrderLineTruck.OrderLineId);

                var scheduleOrderLineDto = await _orderLineRepository.GetAll()
                    .Where(ol => ol.Id == newOrderLineId)
                    .GetScheduleOrders()
                    .FirstAsync();

                ConvertScheduleOrderTimesFromUtc(scheduleOrderLineDto, timezone);

                var remainingUtilization = await GetRemainingTruckUtilizationForOrderLineAsync(scheduleOrderLineDto, truck);
                if (remainingUtilization < originalOrderLineTruck.Utilization || remainingUtilization == 0)
                {
                    continue;
                }

                if (truck.VehicleCategory.IsPowered && truck.VehicleCategory.AssetType != AssetType.Trailer
                    && !truck.IsExternal && !truck.AlwaysShowOnSchedule
                    && (
                        existingDriverAssignments.Any(da => da.TruckId == originalOrderLineTruck.TruckId && da.DriverId == null)
                        || !truck.HasDefaultDriver && !existingDriverAssignments.Any(da => da.TruckId == originalOrderLineTruck.TruckId && da.DriverId != null)
                    )
                )
                {
                    result.SomeTrucksAreNotCopied = true;
                    originalOrderLineTrucks.Remove(originalOrderLineTruck);
                    continue;
                }

                var utilizationToAssign = originalOrderLineTruck.IsDone ?
                        GetTruckUtilization(truck.Id, remainingUtilization) : originalOrderLineTruck.Utilization;

                if (utilizationToAssign == 0)
                {
                    continue;
                }

                passedOrderLineTrucks.Add(new OrderLineTruck
                {
                    OrderLineId = newOrderLineId,
                    TruckId = originalOrderLineTruck.TruckId,
                    DriverId = newDriverId,
                    ParentOrderLineTruckId = originalOrderLineTruck.ParentOrderLineTruckId,
                    Utilization = utilizationToAssign,
                    TimeOnJob = newOrder.Date.Value.AddTimeOrNull(originalOrderLineTruck.TimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone),
                });

                originalOrderLineTrucks.Remove(originalOrderLineTruck);
            }

            if (originalOrderLineTrucks.Any())
            {
                result.ConflictingTrucks = originalOrderLineTrucks.Select(x => x.Truck.TruckCode).ToList();
                if (!input.ProceedOnConflict)
                {
                    result.Completed = false;
                    return result;
                }
            }

            foreach (var passedOrderTruck in passedOrderLineTrucks)
            {
                await _orderLineTruckRepository.InsertAsync(passedOrderTruck);
            }

            result.Completed = true;
            return result;

            // Local functions
            int MapOriginalOrderLineIdToNewOrderLineId(int originalOrderLineId)
            {
                if (input.OrderLineId.HasValue)
                {
                    Debug.Assert(newOrderLines.Count == 1);
                    return newOrderLines.Single().Id;
                }
                var originalOrderLine = originalOrderLines.Single(ol => ol.Id == originalOrderLineId);
                return newOrderLines.Where(ol => ol.LineNumber == originalOrderLine.LineNumber).Select(ol => ol.Id).Single();
            }

            decimal GetTruckUtilization(int truckId, decimal remainingOrderUtilization)
            {
                var truckRemainingUtilization = truckRemainingUtilizationDictionary[truckId];
                if (truckRemainingUtilization.TruckNumber == 0)
                {
                    return 0;
                }
                var utilization = truckRemainingUtilization.TruckNumber == 1 ?
                    truckRemainingUtilization.RemainingUtilization :
                    Math.Round(truckRemainingUtilization.RemainingUtilization / truckRemainingUtilization.TruckNumber, 2);

                if (utilization > remainingOrderUtilization)
                {
                    utilization = remainingOrderUtilization;
                }

                truckRemainingUtilization.RemainingUtilization -= utilization;
                truckRemainingUtilization.TruckNumber--;

                return utilization;
            }

            async Task<bool> AllowLeaseHaulerAndTruckIsLeaseHauler(ScheduleTruckDto truck) =>
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature) && (truck.IsExternal || truck.AlwaysShowOnSchedule);
        }

        public async Task<SetOrderNumberOfTrucksResult> SetOrderLineNumberOfTrucks(SetOrderLineNumberOfTrucksInput input)
        {
            var order = await _orderRepository.GetAll()
                .FirstAsync(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId));

            var orderLine = await _orderLineRepository.GetAll()
                .FirstAsync(ol => ol.Id == input.OrderLineId);

            orderLine.NumberOfTrucks = input.NumberOfTrucks;

            orderLine.RemoveStaggeredTimeIfNeeded();

            await DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(orderLine);

            var hasTrucksWithStaggeredTimeOnJob = await _orderLineTruckRepository.GetAll().AnyAsync(olt => olt.OrderLineId == input.OrderLineId && olt.TimeOnJob != null);

            return new SetOrderNumberOfTrucksResult
            {
                NumberOfTrucks = orderLine.NumberOfTrucks,
                OrderUtilization = await _orderLineScheduledTrucksUpdater.GetOrderLineUtilization(orderLine.Id),
                StaggeredTimeKind = orderLine.StaggeredTimeKind,
                IsTimeStaggered = orderLine.StaggeredTimeKind != StaggeredTimeKind.None || hasTrucksWithStaggeredTimeOnJob,
                IsTimeEditable = orderLine.StaggeredTimeKind == StaggeredTimeKind.None,
            };
        }

        private async Task DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(OrderLine orderLine)
        {
            if ((orderLine.MaterialQuantity ?? 0) == 0 && (orderLine.FreightQuantity ?? 0) == 0 && (orderLine.NumberOfTrucks ?? 0) < 0.01)
            {
                await _orderLineScheduledTrucksUpdater.DeleteOrderLineTrucks(new DeleteOrderLineTrucksInput
                {
                    OrderLineId = orderLine.Id
                });
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task<SetOrderLineScheduledTrucksResult> SetOrderLineScheduledTrucks(SetOrderLineScheduledTrucksInput input)
        {
            var order = await _orderRepository.GetAll()
                .FirstAsync(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId));

            var orderLine = await _orderLineRepository.GetAll()
                .FirstAsync(ol => ol.Id == input.OrderLineId);

            await _orderLineScheduledTrucksUpdater.UpdateScheduledTrucks(orderLine, input.ScheduledTrucks);

            return new SetOrderLineScheduledTrucksResult
            {
                ScheduledTrucks = orderLine.ScheduledTrucks,
                OrderUtilization = await _orderLineScheduledTrucksUpdater.GetOrderLineUtilization(orderLine.Id),
            };
        }

        public async Task<SetOrderLineMaterialQuantityResult> SetOrderLineMaterialQuantity(SetOrderLineMaterialQuantityInput input)
        {
            var order = await _orderRepository.GetAll()
                .Include(o => o.OrderLines)
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId))
                .FirstAsync();
            var orderLine = order.OrderLines.First(ol => ol.Id == input.OrderLineId);
            if (orderLine.MaterialQuantity != input.MaterialQuantity)
            {
                orderLine.MaterialQuantity = input.MaterialQuantity;

                if (!orderLine.IsMaterialPriceOverridden)
                {
                    await EnsureOrderIsNotPaid(order.Id);
                    orderLine.MaterialPrice = orderLine.MaterialPricePerUnit * input.MaterialQuantity ?? 0;
                }

                if (!orderLine.IsQuantityValid())
                {
                    throw new UserFriendlyException(L("QuantityIsRequiredWhenTotalIsSpecified"));
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);

                await DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(orderLine);
            }

            return new SetOrderLineMaterialQuantityResult
            {
                MaterialQuantity = orderLine.MaterialQuantity
            };
        }

        public async Task<SetOrderLineFreightQuantityResult> SetOrderLineFreightQuantity(SetOrderLineFreightQuantityInput input)
        {
            var order = await _orderRepository.GetAll()
                .Include(o => o.OrderLines)
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId))
                .FirstAsync();
            var orderLine = order.OrderLines.First(ol => ol.Id == input.OrderLineId);
            if (orderLine.FreightQuantity != input.FreightQuantity)
            {
                orderLine.FreightQuantity = input.FreightQuantity;

                if (!orderLine.IsFreightPriceOverridden)
                {
                    await EnsureOrderIsNotPaid(order.Id);
                    orderLine.FreightPrice = orderLine.FreightPricePerUnit * input.FreightQuantity ?? 0;
                }

                if (!orderLine.IsQuantityValid())
                {
                    throw new UserFriendlyException(L("QuantityIsRequiredWhenTotalIsSpecified"));
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);

                await DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(orderLine);
            }

            return new SetOrderLineFreightQuantityResult
            {
                FreightQuantity = orderLine.FreightQuantity
            };
        }

        //for any office
        private async Task<bool> IsOrderPaid(int orderId)
        {
            return await _orderRepository.GetAll()
                    .Where(x => x.Id == orderId)
                    .SelectMany(x => x.OrderPayments)
                    .Select(x => x.Payment)
                    .Where(x => !x.IsCancelledOrRefunded)
                    .AnyAsync(x => x.AuthorizationCaptureDateTime != null);
        }

        private async Task EnsureOrderIsNotPaid(int orderId)
        {
            if (await IsOrderPaid(orderId))
            {
                throw new UserFriendlyException(L("CannotChangeRatesAndAmountBecauseOrderIsPaid"));
            }
        }

        public async Task<SetOrderLineLoadsResult> SetOrderLineLoads(SetOrderLineLoadsInput input)
        {
            var orderLine = await _orderLineRepository.GetAll()
                .Include(ol => ol.MaterialUom)
                .FirstAsync(ol => ol.Id == input.OrderLineId)
            ;
            orderLine.Loads = input.Loads;
            if (orderLine.MaterialUom.Name.Equals("tons", StringComparison.InvariantCultureIgnoreCase)
                || orderLine.MaterialUom.Name.Equals("ton", StringComparison.InvariantCultureIgnoreCase))
            {
                orderLine.EstimatedAmount = input.Loads * 20;
            }
            if (orderLine.MaterialUom.Name.Equals("loads", StringComparison.InvariantCultureIgnoreCase)
                || orderLine.MaterialUom.Name.Equals("load", StringComparison.InvariantCultureIgnoreCase))
            {
                orderLine.EstimatedAmount = input.Loads;
            }
            if (orderLine.MaterialUom.Name.Equals("hours", StringComparison.InvariantCultureIgnoreCase)
                || orderLine.MaterialUom.Name.Equals("hour", StringComparison.InvariantCultureIgnoreCase))
            {
                orderLine.EstimatedAmount = orderLine.MaterialQuantity / 2;
            }

            return new SetOrderLineLoadsResult
            {
                Loads = orderLine.Loads,
                EstimatedAmount = orderLine.EstimatedAmount,
            };
        }

        public async Task SetOrderDirections(SetOrderDirectionsInput input)
        {
            var order = await _orderRepository.GetAsync(input.OrderId);
            order.Directions = input.Directions;
        }

        public async Task SetOrderLineNote(SetOrderLineNoteInput input)
        {
            if (!input.OrderLineId.HasValue)
            {
                throw new ArgumentNullException(nameof(input.OrderLineId));
            }

            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId.Value);
            await orderLineUpdater.UpdateFieldAsync(o => o.Note, input.Note);
            await orderLineUpdater.SaveChangesAsync();
        }

        public async Task SetOrderLineTime(SetOrderLineTimeInput input)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);

            var order = await orderLineUpdater.GetOrderAsync();
            var date = order.DeliveryDate ?? await GetToday();
            var timezone = await GetTimezone();

            await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, date.AddTimeOrNull(input.Time)?.ConvertTimeZoneFrom(timezone));

            await orderLineUpdater.SaveChangesAsync();
        }

        public async Task SetOrderLineLoadAtId(SetOrderLineLoadAtIdInput input)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);
            await orderLineUpdater.UpdateFieldAsync(o => o.LoadAtId, input.LoadAtId);
            await orderLineUpdater.SaveChangesAsync();
        }

        public async Task SetOrderLineDeliverToId(SetOrderLineDeliverToIdInput input)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);
            await orderLineUpdater.UpdateFieldAsync(o => o.DeliverToId, input.DeliverToId);
            await orderLineUpdater.SaveChangesAsync();
        }

        public async Task SetOrderLineIsComplete(SetOrderLineIsCompleteInput input)
        {
            if (input.IsCancelled)
            {
                input.IsComplete = true;
            }

            if (input.IsComplete && await OpenDispatchesExist(input.OrderLineId))
            {
                await _dispatchingAppService.CancelOrEndAllDispatches(new CancelOrEndAllDispatchesInput
                {
                    OrderLineId = input.OrderLineId
                });
            }

            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);
            await orderLineUpdater.UpdateFieldAsync(x => x.IsComplete, input.IsComplete);
            await orderLineUpdater.UpdateFieldAsync(x => x.IsCancelled, input.IsComplete && input.IsCancelled);
            var order = await orderLineUpdater.GetOrderAsync();
            var today = await GetToday();

            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(x => x.OrderLineId == input.OrderLineId)
                .ToListAsync();

            if (input.IsComplete)
            {
                if (input.IsCancelled)
                {
                    var tickets = await _ticketRepository.GetAll()
                        .Where(x => x.OrderLineId == input.OrderLineId)
                        .Select(x => new
                        {
                            x.TruckId
                        }).ToListAsync();

                    var dispatches = await _dispatchRepository.GetAll()
                        .Where(x => x.OrderLineId == input.OrderLineId && (x.Status == DispatchStatus.Loaded || x.Status == DispatchStatus.Completed))
                        .Select(x => new
                        {
                            x.TruckId,
                            x.Status
                        }).ToListAsync();

                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        if (!tickets.Any(t => t.TruckId == orderLineTruck.TruckId) && !dispatches.Any(d => d.TruckId == orderLineTruck.TruckId))
                        {
                            await _orderLineTruckRepository.DeleteAsync(orderLineTruck);
                            if (order.DeliveryDate >= today)
                            {
                                orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                            }
                        }
                        else
                        {
                            orderLineTruck.IsDone = true;
                            orderLineTruck.Utilization = 0;
                        }
                    }
                }
                else
                {
                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.IsDone = true;
                        orderLineTruck.Utilization = 0;
                    }
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync(); //save deleted OrderLineTrucks first
            await orderLineUpdater.SaveChangesAsync();
        }

        public async Task<PagedResultDto<SelectListDto>> GetOrderLinesToAssignTrucksToSelectList(GetSelectListIdInput input)
        {
            var orderLine = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == input.Id)
                .Select(ol => new
                {
                    ol.Order.LocationId,
                    ol.Order.DeliveryDate,
                    ol.Order.Shift,
                })
                .FirstAsync();
            return await _orderLineRepository.GetAll()
                .Where(ol =>
                    (ol.Order.LocationId == orderLine.LocationId || ol.SharedOrderLines.Any(sol => sol.OfficeId == orderLine.LocationId)) &&
                    ol.Order.DeliveryDate == orderLine.DeliveryDate &&
                    ol.Order.Shift == orderLine.Shift &&
                    !ol.IsComplete &&
                    ol.Id != input.Id &&
                    ol.ScheduledTrucks.HasValue &&
                    (decimal)ol.ScheduledTrucks.Value > (ol.OrderLineTrucks.Sum(olt => (decimal?)olt.Utilization) ?? 0)
                )
                .Select(ol => new SelectListDto<OrderSelectListInfoDto>
                {
                    Id = ol.Id.ToString(),
                    Name = ol.Order.Customer.Name + ", " + ol.DeliverTo.Name + ", " + ol.DeliverTo.StreetAddress + ", "
                        + ol.DeliverTo.City + ", " + ol.DeliverTo.State + ", " + ol.Service.Service1,
                    Item = new OrderSelectListInfoDto
                    {
                        CustomerName = ol.Order.Customer.Name,
                        ServiceName = ol.Service.Service1,
                        DeliverTo = new LocationSelectListInfoDto
                        {
                            Name = ol.DeliverTo.Name,
                            StreetAddress = ol.DeliverTo.StreetAddress,
                            City = ol.DeliverTo.City,
                            State = ol.DeliverTo.State,
                        }
                    }
                })
                .GetSelectListResult(input, x => new SelectListDto
                {
                    Id = x.Id,
                    Name = x.Item.CustomerName + ", "
                        + Utilities.FormatAddress(x.Item.DeliverTo.Name, x.Item.DeliverTo.StreetAddress, x.Item.DeliverTo.City, x.Item.DeliverTo.State, null) + ", "
                        + x.Item.ServiceName
                });
        }

        public async Task<IList<SelectListDto>> GetTrucksSelectList(int orderLineId)
        {
            return await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == orderLineId && olt.Truck.IsActive && !olt.Truck.IsOutOfService)
                .Select(olt => new SelectListDto()
                {
                    Id = olt.TruckId.ToString(),
                    Name = olt.Truck.TruckCode,
                })
                .ToListAsync();
        }

        public async Task<OrderLineTruckToChangeDriverDto> GetOrderLineTruckToChangeDriverModel(int orderLineTruckId)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.Id == orderLineTruckId)
                .Select(olt => new
                {
                    TruckId = olt.TruckId,
                    OrderLineId = olt.OrderLineId,
                    DriverId = olt.DriverId,
                    DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                    olt.Truck.LocationId,
                    LeaseHaulerId = (int?)olt.Truck.LeaseHaulerTruck.LeaseHaulerId
                })
                .FirstOrDefaultAsync();

            var result = new OrderLineTruckToChangeDriverDto
            {
                HasTicketsOrLoads = await _ticketRepository.GetAll()
                    .AnyAsync(t => t.TruckId == orderLineTruck.TruckId && t.OrderLineId == orderLineTruck.OrderLineId),
                OrderLineTruckId = orderLineTruckId,
                DriverId = orderLineTruck.DriverId,
                DriverName = orderLineTruck.DriverName,
                IsExternal = orderLineTruck.LeaseHaulerId.HasValue,
                LeaseHaulerId = orderLineTruck.LeaseHaulerId
            };

            return result;
        }

        public async Task ReassignTrucks(ReassignTrucksInput input)
        {
            DateTime orderDate = await _orderLineRepository.GetAll().Where(ol => ol.Id == input.SourceOrderLineId).Select(ol => ol.Order.DeliveryDate.Value).FirstAsync();
            DateTime today = await GetToday();
            if (orderDate < today)
            {
                throw new UserFriendlyException("You cannot reassign trucks for past orders");
            }
            var sourceOrderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == input.SourceOrderLineId && input.TruckIds.Contains(olt.TruckId))
                .AsNoTracking()
                .ToListAsync();
            var destinationOrderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == input.DestinationOrderLineId)
                .Select(olt => new
                {
                    olt.TruckId,
                    olt.Utilization,
                    olt.IsDone,
                    olt.Id
                })
                .ToListAsync();
            bool markAsDone = today == orderDate;
            foreach (var sourceOrderLineTruck in sourceOrderLineTrucks)
            {
                var destinationOrderLineTruck = destinationOrderLineTrucks.FirstOrDefault(x => sourceOrderLineTruck.TruckId == x.TruckId);
                if (destinationOrderLineTruck != null && destinationOrderLineTruck.IsDone)
                {
                    var utilization = sourceOrderLineTruck.Utilization;
                    await DeleteOrderLineTruck(sourceOrderLineTruck);
                    await ActivateClosedTruck(sourceOrderLineTruck);
                    await SetOrderLineTruckUtilization(destinationOrderLineTruck.Id, utilization);
                }
                else if (destinationOrderLineTruck == null)
                {
                    await DeleteOrderLineTruck(sourceOrderLineTruck);
                    await CreateOrderLineTruck(sourceOrderLineTruck);
                }
            }

            // Local functions
            async Task DeleteOrderLineTruck(OrderLineTruck sourceOrderLineTruck)
            {
                var deleteOrderLineTruckInput = new DeleteOrderLineTruckInput()
                {
                    OrderLineId = input.SourceOrderLineId,
                    OrderLineTruckId = sourceOrderLineTruck.Id,
                    MarkAsDone = markAsDone,
                };
                await this.DeleteOrderLineTruck(deleteOrderLineTruckInput);
            }

            async Task CreateOrderLineTruck(OrderLineTruck sourceOrderLineTruck)
            {
                var addOrderLineTruckInput = new AddOrderLineTruckInternalInput()
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckId = sourceOrderLineTruck.TruckId,
                    DriverId = sourceOrderLineTruck.DriverId,
                    ParentId = sourceOrderLineTruck.ParentOrderLineTruckId,
                    Utilization = !sourceOrderLineTruck.IsDone ? sourceOrderLineTruck.Utilization : 1,
                };
                var result = await AddOrderLineTruckInternal(addOrderLineTruckInput);
                if (result.IsFailed)
                {
                    throw new UserFriendlyException("There are too many trucks being moved to the new order line. Please increase the scheduled number of trucks or reduce the number of trucks being transferred.");
                }
            }

            async Task ActivateClosedTruck(OrderLineTruck sourceOrderLineTruck)
            {
                var activateClosedTrucksInput = new ActivateClosedTrucksInput
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckIds = new[] { sourceOrderLineTruck.TruckId },
                };
                await ActivateClosedTrucks(activateClosedTrucksInput);
            }

            async Task SetOrderLineTruckUtilization(int orderLineTruckId, decimal utilization)
            {
                var orderLineTruckUtilizationEditDto = new OrderLineTruckUtilizationEditDto
                {
                    OrderLineId = input.DestinationOrderLineId,
                    OrderLineTruckId = orderLineTruckId,
                    Utilization = utilization,
                };
                await SetOrderTruckUtilization(orderLineTruckUtilizationEditDto);
            }
        }

        public async Task AssignTrucks(AssignTrucksInput input)
        {
            var trucks = await _truckRepository.GetAll()
                .Where(x => input.TruckIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.DefaultTrailerId,
                    x.CanPullTrailer
                }).ToListAsync();

            foreach (var truckId in input.TruckIds)
            {
                var addOrderTruckResult = await AddOrderLineTruckInternal(new AddOrderLineTruckInternalInput
                {
                    OrderLineId = input.OrderLineId,
                    TruckId = truckId,
                    Utilization = 1,
                });
                if (addOrderTruckResult.IsFailed)
                {
                    //throw new UserFriendlyException(addOrderTruckResult.ErrorMessage);
                    throw new UserFriendlyException("There are too many trucks being assigned to the new order line. Please increase the scheduled number of trucks or reduce the number of trucks being assigned.");
                }

                var truck = trucks.FirstOrDefault(x => x.Id == truckId);
                if (truck != null && truck.CanPullTrailer && truck.DefaultTrailerId.HasValue && await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule))
                {
                    await AddOrderLineTruckInternal(new AddOrderLineTruckInternalInput
                    {
                        OrderLineId = input.OrderLineId,
                        TruckId = truck.DefaultTrailerId.Value,
                        Utilization = 1,
                        ParentId = addOrderTruckResult.Item.Id
                    });
                }
            }
        }

        public async Task<IList<SelectListDto>> GetClosedTrucksSelectList(int orderLineId)
        {
            return await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == orderLineId && olt.IsDone && !olt.Truck.IsOutOfService && olt.Truck.IsActive)
                .Select(olt => new SelectListDto()
                {
                    Id = olt.TruckId.ToString(),
                    Name = olt.Truck.TruckCode,
                })
                .ToListAsync();
        }

        public async Task<bool> ActivateClosedTrucks(ActivateClosedTrucksInput input)
        {
            var orderLineTrucksToActivate = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == input.OrderLineId && input.TruckIds.Contains(olt.TruckId) && olt.Truck.IsActive && !olt.Truck.IsOutOfService)
                .ToListAsync();
            bool result = true;
            foreach (OrderLineTruck orderLineTruck in orderLineTrucksToActivate)
            {
                decimal remainedUtilization = await GetRemainedUtilizationForTruck(orderLineTruck.TruckId, orderLineTruck.OrderLineId);
                if (remainedUtilization == 0)
                {
                    result = false;
                    continue;
                }
                orderLineTruck.IsDone = false;
                orderLineTruck.Utilization = remainedUtilization;
            }

            return result;

            // Local functions
            async Task<decimal> GetRemainedUtilizationForTruck(int truckId, int orderLineId)
            {
                var dateShift = await _orderLineRepository.GetAll()
                    .Where(ol => ol.Id == orderLineId)
                    .Select(ol => new
                    {
                        Date = ol.Order.DeliveryDate,
                        ol.Order.Shift,
                    })
                    .FirstAsync();
                decimal truckUtilization = await _orderLineTruckRepository.GetAll()
                    .Where(olt =>
                        olt.TruckId == truckId &&
                        olt.OrderLine.Order.DeliveryDate == dateShift.Date &&
                        olt.OrderLine.Order.Shift == dateShift.Shift
                    )
                    .SumAsync(olt => olt.Utilization);
                return 1 - truckUtilization;
            }
        }

        public async Task<bool> OpenDispatchesExist(int orderLineId) =>
            await _orderLineRepository.GetAll()
                .AnyAsync(ol => ol.Id == orderLineId && ol.Dispatches.Any(d => !Dispatch.ClosedDispatchStatuses.Contains(d.Status)));

        public async Task<SetOrderOfficeIdInput> GetOrderOfficeIdForEdit(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.Id))
                .Select(o => new SetOrderOfficeIdInput
                {
                    OrderId = o.Id,
                    OfficeId = o.LocationId,
                    OfficeName = o.Office.Name,
                    OrderLineId = o.OrderLines.Count == 1 ? (int?)null : input.Id,
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            return order;
        }

        public async Task<SetOrderDirectionsInput> GetOrderDirectionsForEdit(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.Id))
                .Select(o => new SetOrderDirectionsInput
                {
                    OrderId = o.Id,
                    Directions = o.Directions
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            return order;
        }

        public async Task<SetOrderLineNoteInput> GetOrderLineNoteForEdit(NullableIdDto input)
        {
            if (!input.Id.HasValue)
            {
                return new SetOrderLineNoteInput();
            }

            var orderLine = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == input.Id)
                .Select(ol => new SetOrderLineNoteInput
                {
                    OrderLineId = ol.Id,
                    Note = ol.Note
                })
                .FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw await GetOrderLineNotFoundException(new EntityDto(input.Id.Value));
            }

            return orderLine;
        }

        public async Task<SetOrderDateInput> GetSetOrderDateInput(int orderLineId)
        {
            var setOrderDateInput = await _orderRepository.GetAll()
                .Where(o => o.OrderLines.Any(ol => ol.Id == orderLineId))
                .Select(o => new SetOrderDateInput
                {
                    OrderId = o.Id,
                    Date = o.DeliveryDate.Value,
                    OrderLineId = o.OrderLines.Count == 1 ? (int?)null : orderLineId,
                })
                .FirstOrDefaultAsync();

            return setOrderDateInput;
        }

        public async Task<OrderLineTruckDetailsDto> GetOrderTruckUtilizationForEdit(EntityDto input)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.Id == input.Id)
                .Select(olt => new
                {
                    olt.Utilization,
                    olt.OrderLineId,
                    olt.Truck.VehicleCategory.IsPowered,
                    olt.Truck.VehicleCategory.AssetType,
                    olt.TruckId,
                    olt.Truck.TruckCode,
                    TimeOnJobUtc = olt.TimeOnJob
                })
                .FirstOrDefaultAsync();

            if (orderLineTruck == null)
            {
                throw await GetOrderLineTruckNotFoundException(input);
            }

            var orderLine = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == orderLineTruck.OrderLineId)
                .GetScheduleOrders()
                .Select(x => new
                {
                    x.Utilization,
                    x.ScheduledTrucks,
                    x.Date,
                    x.Shift,
                })
                .FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw await GetOrderLineNotFoundException(new EntityDto(orderLineTruck.OrderLineId));
            }

            var orderLineMaxUtilization = orderLine.ScheduledTrucks.HasValue ? Convert.ToDecimal(orderLine.ScheduledTrucks.Value) : 0;

            var currentTruckUtilization = orderLineTruck.Utilization;
            var truckUtilization = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.TruckId == orderLineTruck.TruckId && olt.OrderLine.Order.DeliveryDate == orderLine.Date && olt.OrderLine.Order.Shift == orderLine.Shift)
                .SumAsync(olt => olt.Utilization);
            var remainingTruckUtilization = await GetRemainingTruckUtilizationForOrderAsync(new GetRemainingTruckUtilizationForOrderInput
            {
                OrderMaxUtilization = orderLineMaxUtilization,
                OrderUtilization = orderLine.Utilization,
                AssetType = orderLineTruck.AssetType,
                IsPowered = orderLineTruck.IsPowered,
                TruckUtilization = truckUtilization,
            });
            var maxUtilization = Math.Min(1, currentTruckUtilization + remainingTruckUtilization);
            return new OrderLineTruckDetailsDto
            {
                OrderLineTruckId = input.Id,
                OrderLineId = orderLineTruck.OrderLineId,
                Utilization = orderLineTruck.Utilization,
                MaxUtilization = maxUtilization,
                TruckCode = orderLineTruck.TruckCode,
                TimeOnJob = orderLineTruck.TimeOnJobUtc?.ConvertTimeZoneTo(await GetTimezone())
            };
        }

        private async Task<Exception> GetOrderLineTruckNotFoundException(EntityDto input)
        {
            var deletedOrderLineTruck = await _orderLineTruckRepository.GetDeletedEntity(input, CurrentUnitOfWork);
            if (deletedOrderLineTruck == null)
            {
                return new Exception($"OrderLineTruck with id {input.Id} wasn't found and is not deleted");
            }

            if (await _orderRepository.IsEntityDeleted(new EntityDto(deletedOrderLineTruck.OrderLine.OrderId), CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            return new EntityDeletedException("OrderTruck", "This order truck has been deleted and can’t be edited");
        }

        private async Task<Exception> GetOrderNotFoundException(EntityDto input)
        {
            if (await _orderRepository.IsEntityDeleted(input, CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            return new Exception($"Order with id {input.Id} wasn't found and is not deleted");
        }

        private async Task<Exception> GetOrderLineNotFoundException(EntityDto input)
        {
            if (await _orderLineRepository.IsEntityDeleted(input, CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order Line", "This order line has been deleted and can’t be edited");
            }

            return new Exception($"Order Line with id {input.Id} wasn't found and is not deleted");
        }

        public async Task SetOrderLineTruckDetails(OrderLineTruckDetailsDto input)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAsync(input.OrderLineTruckId);

            var orderDetails = await _orderLineRepository.GetAll()
                .Where(x => x.Id == orderLineTruck.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate
                }).FirstAsync();

            var date = orderDetails.DeliveryDate ?? await GetToday();

            var newValue = date.AddTimeOrNull(input.TimeOnJob)?.ConvertTimeZoneFrom(await GetTimezone());
            if (newValue == orderLineTruck.TimeOnJob)
            {
                return;
            }
            orderLineTruck.TimeOnJob = newValue;
        }

        public async Task SetOrderTruckUtilization(OrderLineTruckUtilizationEditDto input)
        {
            var originalEditDto = await GetOrderTruckUtilizationForEdit(new EntityDto(input.OrderLineTruckId));
            var orderLineTruck = await _orderLineTruckRepository.GetAsync(input.OrderLineTruckId);

            if (input.Utilization <= 0)
            {
                await DeleteOrderLineTruck(new DeleteOrderLineTruckInput
                {
                    OrderLineTruckId = input.OrderLineTruckId,
                    OrderLineId = orderLineTruck.OrderLineId
                });
            }
            else
            {
                if (input.Utilization > originalEditDto.MaxUtilization)
                {
                    input.Utilization = originalEditDto.MaxUtilization;
                }
                orderLineTruck.Utilization = input.Utilization;
            }

        }

        public async Task ChangeOrderLineUtilization(ChangeOrderLineUtilizationInput input)
        {
            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Include(t => t.Truck)
                    .ThenInclude(t => t.VehicleCategory)
                .Where(t => t.OrderLineId == input.OrderLineId && !t.IsDone && t.Truck.VehicleCategory.IsPowered)
                .ToListAsync();

            if (!orderLineTrucks.Any())
            {
                return;
            }

            var orderLine = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == input.OrderLineId)
                .GetScheduleOrders()
                .Select(x => new
                {
                    //x.Utilization,
                    x.ScheduledTrucks,
                    x.Date,
                    x.Shift,
                })
                .FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw await GetOrderLineNotFoundException(new EntityDto(input.OrderLineId));
            }

            if (input.Utilization <= 0)
            {
                var existingDispatches = await OrderLineHasDispatches(new DeleteOrderLineTruckInput { OrderLineId = input.OrderLineId });

                var hasAcknowledgedOrLoadedDispatches = existingDispatches.FirstOrDefault(t => t.AcknowledgedOrLoaded);
                if (hasAcknowledgedOrLoadedDispatches != null)
                {
                    throw new UserFriendlyException(L("TruckHasDispatch_YouMustCancelItFirstToRemoveTruck", hasAcknowledgedOrLoadedDispatches.TruckCode));
                }

                await _dispatchingAppService.CancelOrEndAllDispatches(new CancelOrEndAllDispatchesInput
                {
                    OrderLineId = input.OrderLineId,
                });

                await _orderLineTruckRepository.DeleteAsync(x => x.OrderLineId == input.OrderLineId);

                return;
            }

            var orderLineMaxUtilization = orderLine.ScheduledTrucks.HasValue ? Convert.ToDecimal(orderLine.ScheduledTrucks.Value) : 0;
            var orderLineUtilization = 0M;

            foreach (var orderLineTruck in orderLineTrucks)
            {
                var currentTruckUtilization = orderLineTruck.Utilization;
                var truckUtilization = await _orderLineTruckRepository.GetAll()
                    .Where(olt => olt.TruckId == orderLineTruck.TruckId && olt.OrderLine.Order.DeliveryDate == orderLine.Date && olt.OrderLine.Order.Shift == orderLine.Shift)
                    .SumAsync(olt => olt.Utilization);
                var remainingTruckUtilization = await GetRemainingTruckUtilizationForOrderAsync(new GetRemainingTruckUtilizationForOrderInput
                {
                    OrderMaxUtilization = orderLineMaxUtilization,
                    OrderUtilization = orderLineUtilization,
                    AssetType = orderLineTruck.Truck.VehicleCategory.AssetType,
                    IsPowered = orderLineTruck.Truck.VehicleCategory.IsPowered,
                    TruckUtilization = truckUtilization,
                });
                var maxTruckUtilization = Math.Min(1, currentTruckUtilization + remainingTruckUtilization);

                if (input.Utilization > maxTruckUtilization)
                {
                    throw new UserFriendlyException(L("TruckCantHaveUtilizationHigherThan", orderLineTruck.Truck.TruckCode, Math.Round(maxTruckUtilization, 2)));
                }

                orderLineUtilization += input.Utilization;
                if (orderLineUtilization > orderLineMaxUtilization)
                {
                    throw new UserFriendlyException(L("UtilizationWouldExceedNumberOfTrucks"));
                }

                orderLineTruck.Utilization = input.Utilization;
            }
        }

        private async Task<decimal> GetRemainingTruckUtilizationForOrderLineAsync(ScheduleOrderLineDto orderLine, ScheduleTruckDto truck)
        {
            return await GetRemainingTruckUtilizationForOrderAsync(GetRemainingTruckUtilizationForOrderInput.From(orderLine, truck));
        }

        private async Task<decimal> GetRemainingTruckUtilizationForOrderAsync(GetRemainingTruckUtilizationForOrderInput input)
        {
            if (!await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization))
            {
                return 1;
            }

            if (input.OrderMaxUtilization == 0) return 0;

            if (!input.IsPowered)
            {
                //previous trailer utilization logic
                //return input.TruckUtilization > 0 ? 0 : 1;
                return 1;
            }

            var remainToUtilize = input.OrderMaxUtilization - input.OrderUtilization;
            if (remainToUtilize <= 0) return 0;
            if (input.TruckUtilization >= 1) return 0;

            return Math.Min((1 - input.TruckUtilization), remainToUtilize);
        }

        public async Task<string> GetDeviceIdsStringForOrderLineTrucks(int orderLineId)
        {
            var truckCodes = await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == orderLineId)
                .Select(olt => olt.Truck.TruckCode)
                .Distinct()
                .ToArrayAsync();
            return (await _telematics.GetDeviceIdsByTruckCodesAsync(truckCodes)).JoinAsString(",");
        }

        [HttpPost]
        public async Task<LeaseHaulerSelectionModel> GetLeaseHaulerSelectionModel(GetLeaseHaulerSelectionModelInput input)
        {
            var result = new LeaseHaulerSelectionModel();

            result.Rows = await _availableLeaseHaulerTruckRepository.GetAll()
                    .Where(x => x.Date == input.Date
                        && x.Shift == input.Shift
                        && x.OfficeId == input.OfficeId)
                    .Select(x => new LeaseHaulerSelectionRowDto
                    {
                        Id = x.Id,
                        LeaseHaulerId = x.LeaseHaulerId,
                        LeaseHaulerName = x.LeaseHauler.Name,
                        TruckId = x.TruckId,
                        TruckCode = x.Truck.TruckCode,
                        DriverId = x.DriverId,
                        DriverName = x.Driver.FirstName + " " + x.Driver.LastName
                    }).ToListAsync();

            result.LeaseHaulers = await _leaseHaulerRepository.GetAll()
                .Where(x => x.IsActive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            if (result.Rows.Any())
            {
                var leaseHaulerIds = result.Rows.Select(x => x.LeaseHaulerId).Distinct().ToList();

                result.Trucks = await GetLeaseHaulerTrucks(new IdListInput(leaseHaulerIds));

                result.Drivers = await GetLeaseHaulerDrivers(new IdListInput(leaseHaulerIds));
            }

            return result;
        }

        public async Task<List<LeaseHaulerSelectionTruckDto>> GetLeaseHaulerTrucks(IdListInput input)
        {
            return await _truckRepository.GetAll()
                    .Where(x => x.IsActive && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true && input.Ids.Contains(x.LeaseHaulerTruck.LeaseHaulerId))
                    .Select(x => new LeaseHaulerSelectionTruckDto
                    {
                        LeaseHaulerId = x.LeaseHaulerTruck.LeaseHaulerId,
                        TruckId = x.Id,
                        TruckCode = x.TruckCode,
                        DefaultDriverId = x.DefaultDriverId == null ? 0 : x.DefaultDriverId.Value
                    })
                    .OrderBy(x => x.LeaseHaulerId)
                    .ThenBy(x => x.TruckCode)
                    .ToListAsync();
        }

        public async Task<List<LeaseHaulerSelectionDriverDto>> GetLeaseHaulerDrivers(IdListInput input)
        {
            return await _driverRepository.GetAll()
                    .Where(x => input.Ids.Contains(x.LeaseHaulerDriver.LeaseHaulerId) && !x.IsInactive)
                    .Select(x => new LeaseHaulerSelectionDriverDto
                    {
                        LeaseHaulerId = x.LeaseHaulerDriver.LeaseHaulerId,
                        DriverId = x.Id,
                        DriverName = x.FirstName + " " + x.LastName
                    })
                    .OrderBy(x => x.LeaseHaulerId)
                    .ThenBy(x => x.DriverName)
                    .ToListAsync();
        }

        public async Task UpdateLeaseHaulerSelection(LeaseHaulerSelectionDto model)
        {
            var existingRecords = await _availableLeaseHaulerTruckRepository.GetAll()
                .Where(x => x.Date == model.Date
                        && x.Shift == model.Shift
                        && x.OfficeId == model.OfficeId)
                .WhereIf(model.LeaseHaulerId.HasValue, x => x.LeaseHaulerId == model.LeaseHaulerId)
                .ToListAsync();

            if (model.AddAllTrucks)
            {
                var allTrucks = await _truckRepository.GetAll()
                    .Where(x => x.IsActive && x.LocationId == null && x.DefaultDriverId != null && x.LeaseHaulerTruck != null)
                    .Select(x => new
                    {
                        LeaseHaulerId = x.LeaseHaulerTruck.LeaseHaulerId,
                        TruckId = x.Id,
                        DriverId = x.DefaultDriverId.Value
                    }).ToListAsync();

                foreach (var truck in allTrucks)
                {
                    if (existingRecords.Any(x => x.TruckId == truck.TruckId))
                    {
                        continue;
                    }

                    await _availableLeaseHaulerTruckRepository.InsertAsync(new AvailableLeaseHaulerTruck
                    {
                        OfficeId = model.OfficeId,
                        Date = model.Date,
                        Shift = model.Shift,
                        LeaseHaulerId = truck.LeaseHaulerId,
                        TruckId = truck.TruckId,
                        DriverId = truck.DriverId
                    });
                }
            }
            else
            {
                if (model.Rows == null)
                {
                    throw new ArgumentException(nameof(model.Rows));
                }

                if (model.Rows.Select(x => x.TruckId).Distinct().Count() < model.Rows.Count)
                {
                    throw new UserFriendlyException("You selected the same truck twice");
                }

                foreach (var row in model.Rows)
                {
                    if (row.Id != 0)
                    {
                        var existingRow = existingRecords.FirstOrDefault(x => x.Id == row.Id);
                        if (existingRow != null)
                        {
                            existingRow.TruckId = row.TruckId;
                            existingRow.DriverId = row.DriverId;
                            continue;
                        }
                        row.Id = 0;
                    }

                    await _availableLeaseHaulerTruckRepository.InsertAsync(new AvailableLeaseHaulerTruck
                    {
                        OfficeId = model.OfficeId,
                        Date = model.Date,
                        Shift = model.Shift,
                        LeaseHaulerId = row.LeaseHaulerId,
                        TruckId = row.TruckId,
                        DriverId = row.DriverId
                    });
                }

                var rowsToDelete = existingRecords.Where(e => !model.Rows.Any(r => r.Id != 0 && r.Id == e.Id)).ToList();
                rowsToDelete.ForEach(_availableLeaseHaulerTruckRepository.Delete);

                var orderLineTrucksToDelete = await _orderLineTruckRepository.GetAll()
                    .Where(olt => olt.OrderLine.Order.DeliveryDate == model.Date
                                  && olt.OrderLine.Order.Shift == model.Shift
                                  && olt.OrderLine.Order.LocationId == model.OfficeId
                                  && rowsToDelete.Any(alht => alht.TruckId == olt.TruckId)
                    )
                    .ToListAsync();
                orderLineTrucksToDelete.ForEach(_orderLineTruckRepository.Delete);

                await CurrentUnitOfWork.SaveChangesAsync();
                var orderLineIds = orderLineTrucksToDelete.Select(x => x.OrderLineId).Distinct().ToList();
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

        public async Task SetAllOrderLinesIsComplete(GetScheduleOrdersInput input)
        {
            input.HideCompletedOrders = true;
            var query = await GetScheduleQueryAsync(input);

            var items = await query
                .Select(x => new
                {
                    x.Id
                })
                .ToListAsync();

            foreach (var item in items)
            {
                await SetOrderLineIsComplete(new SetOrderLineIsCompleteInput() { OrderLineId = item.Id, IsComplete = true, IsCancelled = false });
            }
        }

        public async Task<SendOrderLineToHaulingCompanyInput> GetInputForSendOrderLineToHaulingCompany(int orderLineId)
        {
            return await _crossTenantOrderSender.GetInputForSendOrderLineToHaulingCompany(orderLineId);
        }

        public async Task SendOrderLineToHaulingCompany(SendOrderLineToHaulingCompanyInput input)
        {
            await _crossTenantOrderSender.SendOrderLineToHaulingCompany(input);
        }

        public async Task TryGetOrderTrucksAndDetailsOrThrow(int orderLineId)
        {
            var timeZone = await GetTimezone();
            var orderLineTrucksQuery = _orderLineRepository.GetAll()
                        .Where(ol => ol.Id == orderLineId)
                        .SelectMany(ol => ol.OrderLineTrucks);

            if (!await orderLineTrucksQuery.AnyAsync())
            {
                throw new UserFriendlyException("Order line queried is not found.");
            }

            var dispatches = orderLineTrucksQuery.SelectMany(p => p.Dispatches).ToList();
            if (dispatches == null || !dispatches.Any())
            {
                throw new UserFriendlyException("No dispatch has yet been performed for this order.");
            }

            var loads = orderLineTrucksQuery.SelectMany(p => p.Dispatches).SelectMany(d => d.Loads).ToList();
            if (loads == null || !loads.Any() || loads.All(p => p.SourceDateTime == null && p.DestinationDateTime == null))
            {
                throw new UserFriendlyException("Driver is dispatched but no truck has yet been loaded.");
            }
        }

        private static void TrimTimeEntries(List<Drivers.EmployeeTime> timeEntries, DateTime? sourceDateTime)
        {
            if (timeEntries.Count <= 2)
                return;

            var timeEntriesToRemove = new List<int>();
            for (var ctr = 0; ctr < timeEntries.Count; ctr++)
            {
                var thisTimeEntry = timeEntries[ctr];
                var nextNextTimeEntry = (ctr >= (timeEntries.Count - 2)) ? null : timeEntries[ctr + 2];

                if (nextNextTimeEntry != null &&
                    nextNextTimeEntry.StartDateTime < sourceDateTime)
                {
                    timeEntriesToRemove.Add(thisTimeEntry.Id);
                }
            }
            if (timeEntriesToRemove.Any())
                timeEntries.RemoveAll(p => timeEntriesToRemove.Contains(p.Id));
        }

        private async Task<IList<OrderTruckQueryResultDto>> GetOrderTrucksGroupedAsync_Old(int orderLineId)
        {
            var orderLineTrucksQuery = _orderLineRepository.GetAll()
                        .Where(ol => ol.Id == orderLineId)
                        .SelectMany(ol => ol.OrderLineTrucks);

            var dispatchesQuery = orderLineTrucksQuery.SelectMany(olt => olt.Dispatches);

            var loadsQuery = dispatchesQuery.Where(d => d.Loads != null && d.Loads.Any())
                                                .SelectMany(dispatch => dispatch.Loads);

            var orderTrucksQuery = loadsQuery.Where(load => load != null && load.SourceDateTime.HasValue)
                                        .Select(load => new
                                        {
                                            load.Dispatch.OrderLineTruck.TruckId,
                                            load.Dispatch.OrderLineTruck.Truck.TruckCode,
                                            load.Dispatch.OrderLineTruck.DriverId,

                                            DispatchId = load.Dispatch.Id,

                                            LoadId = load.Id,
                                            load.SourceDateTime,
                                            load.DestinationDateTime,
                                            load.SourceLatitude,
                                            load.SourceLongitude,
                                            load.DestinationLatitude,
                                            load.DestinationLongitude,

                                            TicketId = load.Tickets.Count > 0 ? load.Tickets.FirstOrDefault().Id : 0,
                                            Qty = load.Tickets.Count > 0 ? load.Tickets.FirstOrDefault().Quantity : 0,
                                            //UoMId = load.Tickets.FirstOrDefault().UnitOfMeasureId,
                                            //UoMName = load.Tickets.FirstOrDefault().UnitOfMeasure.Name
                                            UoMId = load.Dispatch.OrderLineTruck.OrderLine.FreightUomId,
                                            UoMName = load.Dispatch.OrderLineTruck.OrderLine.FreightUom.Name
                                        });

            var orderTrucks = await orderTrucksQuery.ToListAsync();
            var list = orderTrucks
                .GroupBy(p => new
                {
                    p.TruckId,
                    p.UoMName
                })
                .Select(p => new OrderTruckQueryResultDto
                {
                    TruckId = p.Key.TruckId,
                    TruckCode = p.Select(t => t.TruckCode).FirstOrDefault(),
                    UnitOfMeasure = p.Key.UoMName,
                    LoadsCount = p.Count(),
                    Quantity = p.Sum(x => x.Qty),
                    Loads = p.Select(load => new OrderTruckQueryResultDto.OrderTruckLoadQueryResultDto
                    {
                        LoadId = load.LoadId,
                        DriverId = load.DriverId,
                        SourceDateTime = load.SourceDateTime,
                        DestinationDateTime = load.DestinationDateTime,
                        SourceCoordinates = new double?[] { load.SourceLatitude, load.SourceLongitude },
                        DestinationCoordinates = new double?[] { load.DestinationLatitude, load.DestinationLongitude },
                    })
                    .OrderBy(p => p.SourceDateTime)
                    .ThenBy(p => p.DestinationDateTime)
                    .ToList()
                }).ToList();

            return list;
        }

        private async Task<IList<OrderTruckQueryResultDto>> GetOrderTrucksGroupedAsync(int orderLineId)
        {
            var orderLineTrucksQuery = _orderLineRepository.GetAll()
                                            .Where(ol => ol.Id == orderLineId)
                                            .SelectMany(ol => ol.OrderLineTrucks
                                                .SelectMany(olt => olt.Dispatches
                                                        .Where(d => d.Loads != null && d.Loads.Any())
                                                        .SelectMany(dispatch => dispatch.Loads)
                                                            .Where(load => load != null && load.SourceDateTime.HasValue)
                                                            .Select(load => new
                                                            {
                                                                olt.TruckId,
                                                                olt.Truck.TruckCode,
                                                                olt.DriverId,

                                                                DispatchId = load.Dispatch.Id,

                                                                LoadId = load.Id,
                                                                load.SourceDateTime,
                                                                load.DestinationDateTime,
                                                                load.SourceLatitude,
                                                                load.SourceLongitude,
                                                                load.DestinationLatitude,
                                                                load.DestinationLongitude,

                                                                TicketId = load.Tickets.Count > 0 ? load.Tickets.FirstOrDefault().Id : 0,
                                                                Qty = load.Tickets.Count > 0 ? load.Tickets.FirstOrDefault().Quantity : 0,
                                                                //UoMId = load.Tickets.FirstOrDefault().UnitOfMeasureId,
                                                                //UoMName = load.Tickets.FirstOrDefault().UnitOfMeasure.Name
                                                                UoMId = olt.OrderLine.FreightUomId,
                                                                UoMName = olt.OrderLine.FreightUom.Name
                                                            })));

            var orderTrucks = await orderLineTrucksQuery.ToListAsync();
            var list = orderTrucks
                        .GroupBy(p => new
                        {
                            p.TruckId,
                            p.UoMName
                        })
                        .Select(p => new OrderTruckQueryResultDto
                        {
                            TruckId = p.Key.TruckId,
                            TruckCode = p.Select(t => t.TruckCode).FirstOrDefault(),
                            UnitOfMeasure = p.Key.UoMName,
                            LoadsCount = p.Count(),
                            Quantity = p.Sum(x => x.Qty),
                            Loads = p.Select(load => new OrderTruckQueryResultDto.OrderTruckLoadQueryResultDto
                            {
                                LoadId = load.LoadId,
                                DriverId = load.DriverId,
                                SourceDateTime = load.SourceDateTime,
                                DestinationDateTime = load.DestinationDateTime,
                                SourceCoordinates = new double?[] { load.SourceLatitude, load.SourceLongitude },
                                DestinationCoordinates = new double?[] { load.DestinationLatitude, load.DestinationLongitude },
                            })
                            .OrderBy(p => p.SourceDateTime)
                            .ThenBy(p => p.DestinationDateTime)
                            .ToList()
                        }).ToList();

            return list;
        }

        public async Task<OrderTrucksDto> GetOrderTrucksAndDetails(int orderLineId)
        {
            await TryGetOrderTrucksAndDetailsOrThrow(orderLineId);

            var timeZone = await GetTimezone();
            var orderTrucks = await GetOrderTrucksGroupedAsync(orderLineId);

            var allTripsEarliest = orderTrucks.SelectMany(p => p.Loads)
                                            .Where(p => p.SourceDateTime.HasValue)
                                            .Select(p => p.SourceDateTime)
                                            .Min();

            var allTripsLatest = orderTrucks.SelectMany(p => p.Loads)
                                            .Where(p => p.DestinationDateTime.HasValue)
                                            .Select(p => p.DestinationDateTime)
                                            .Max();

            var tryLatestFromSources = orderTrucks.SelectMany(p => p.Loads)
                                            .Where(p => p.SourceDateTime.HasValue)
                                            .Select(p => p.SourceDateTime)
                                            .Max();

            // it may be that latest of trip is not done yet so set latest
            // time to start of the last of the trips.
            if ((tryLatestFromSources.HasValue && allTripsLatest.HasValue &&
                tryLatestFromSources.Value > allTripsLatest.Value) ||
                !allTripsLatest.HasValue && tryLatestFromSources.HasValue)
            {
                allTripsLatest = tryLatestFromSources;
            }

            var earliest = allTripsEarliest ?? default;
            var latest = allTripsLatest ?? default;

            var orderTrucksDto = new OrderTrucksDto()
            {
                Earliest = earliest.AddMinutes(-30),
                Latest = latest.AddMinutes(30)
            };

            var allDriverIds = orderTrucks.SelectMany(p => p.Loads)
                                            .Select(p => p.DriverId)
                                            .Distinct()
                                            .ToList();

            var allEmployeeTimes = await _employeeTimeRepository
                                    .GetAll()
                                    .Where(p => !p.IsImported
                                                && !p.ManualHourAmount.HasValue
                                                && allDriverIds.Contains(p.DriverId)
                                                && p.LastModificationTime.HasValue
                                                )
                                    .Where(p => p.StartDateTime >= earliest.AddHours(-1) && p.EndDateTime <= latest.AddHours(1))
                                    .OrderBy(p => p.Id)
                                    .ToListAsync();

            if (allEmployeeTimes == null || !allEmployeeTimes.Any())
            {
                orderTrucksDto.OrderTrucks = new List<OrderTruckDto>();
                return orderTrucksDto;
            }

            foreach (var orderTruck in orderTrucks)
            {
                var orderTruckDto = new OrderTruckDto()
                {
                    TruckId = orderTruck.TruckId,
                    TruckCode = orderTruck.TruckCode,
                    UnitOfMeasure = orderTruck.UnitOfMeasure,
                    LoadsCount = orderTruck.LoadsCount,
                    Quantity = orderTruck.Quantity,
                };

                var orderTruckDriverIds = orderTruck.Loads
                                            .Where(p => p.DriverId.HasValue)
                                            .Select(p => p.DriverId)
                                            .Distinct().ToList();

                var employeeTimes = allEmployeeTimes
                                            .Where(p => orderTruckDriverIds.Contains(p.DriverId))
                                            .ToList();

                if (employeeTimes == null || !employeeTimes.Any())
                {
                    orderTruckDto.TripCycles = new List<TripCycleDto>();
                    continue;
                }

                TrimTimeEntries(employeeTimes, orderTruck.Loads[0].SourceDateTime);
                var timeEntriesQueue = new Queue<Drivers.EmployeeTime>(employeeTimes);
                var checkTimeEntry = timeEntriesQueue.Dequeue();

                for (var ctr = 0; ctr < orderTruck.Loads.Count; ctr++)
                {
                    var prevLoad = ctr == 0 ? null : orderTruck.Loads[ctr - 1];
                    var thisLoad = orderTruck.Loads[ctr];
                    var nextLoad = ctr < (orderTruck.Loads.Count - 1) ? orderTruck.Loads[ctr + 1] : null;

                    if (ctr > 0 && prevLoad != null && checkTimeEntry != null &&
                        checkTimeEntry.EndDateTime >= prevLoad.DestinationDateTime &&
                        checkTimeEntry.EndDateTime <= thisLoad.SourceDateTime)
                    {
                        checkTimeEntry = null;
                    }

                    // to Load site
                    var tripCycleDto = new TripCycleDto()
                    {
                        CycleId = $"{thisLoad.LoadId}-0-{ctr + 1}",
                        TruckTripType = TruckTripTypesEnum.TripToLoadSite,
                        DriverId = thisLoad.DriverId,
                        SourceCoordinates = thisLoad.SourceCoordinates,
                        DestinationCoordinates = thisLoad.DestinationCoordinates,
                        Label = $"#{orderTruckDto.TripCycles.Count + 1}"
                    };

                    if (ctr == 0 || (checkTimeEntry == null && timeEntriesQueue.Any()))
                    {
                        checkTimeEntry ??= timeEntriesQueue.Dequeue();

                        tripCycleDto.StartDateTime = checkTimeEntry.StartDateTime;

                        if (timeEntriesQueue.Any())
                            checkTimeEntry = timeEntriesQueue.Dequeue();
                        else checkTimeEntry = null;
                    }
                    else
                    {
                        tripCycleDto.StartDateTime = orderTruck.Loads[ctr - 1].DestinationDateTime;
                    }

                    tripCycleDto.EndDateTime = thisLoad.SourceDateTime;

                    orderTruckDto.TripCycles.Add(tripCycleDto);

                    // to Dump site
                    if (!thisLoad.DestinationDateTime.HasValue)
                        continue;

                    tripCycleDto = new TripCycleDto()
                    {
                        CycleId = $"{thisLoad.LoadId}-1-{ctr + 1}",
                        TruckTripType = TruckTripTypesEnum.TripToDeliverySite,
                        DriverId = thisLoad.DriverId,
                        SourceCoordinates = thisLoad.SourceCoordinates,
                        DestinationCoordinates = thisLoad.DestinationCoordinates,
                        Label = $"#{orderTruckDto.TripCycles.Count + 1}"
                    };

                    if (checkTimeEntry != null &&
                        thisLoad.SourceDateTime <= checkTimeEntry.EndDateTime &&
                        (checkTimeEntry.EndDateTime <= thisLoad.DestinationDateTime || nextLoad != null && checkTimeEntry.EndDateTime <= nextLoad.SourceDateTime))
                    {
                        tripCycleDto.StartDateTime = checkTimeEntry.EndDateTime;

                        if (timeEntriesQueue.Any())
                            checkTimeEntry = timeEntriesQueue.Dequeue();
                    }
                    else
                    {
                        tripCycleDto.StartDateTime = thisLoad.SourceDateTime;
                    }

                    tripCycleDto.EndDateTime = thisLoad.DestinationDateTime;

                    orderTruckDto.TripCycles.Add(tripCycleDto);
                }
                orderTrucksDto.OrderTrucks.Add(orderTruckDto);
            }

            return orderTrucksDto;
        }
    }
}
