using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.RepositoryExtensions;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverAssignments
{
    [AbpAuthorize]
    public class DriverAssignmentAppService : DispatcherWebAppServiceBase, IDriverAssignmentAppService
    {
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly ISyncRequestSender _syncRequestSender;

        public DriverAssignmentAppService(
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Order> orderRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Office> officeRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<TimeOff> timeOffRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            IDriverApplicationPushSender driverApplicationPushSender,
            IDriverApplicationLogger driverApplicationLogger,
            ISyncRequestSender syncRequestSender
            )
        {
            _driverAssignmentRepository = driverAssignmentRepository;
            _orderRepository = orderRepository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _officeRepository = officeRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _driverApplicationPushSender = driverApplicationPushSender;
            _driverApplicationLogger = driverApplicationLogger;
            _syncRequestSender = syncRequestSender;
            _timeOffRepository = timeOffRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        [HttpPost]
        public async Task<ListResultDto<DriverAssignmentLiteDto>> GetAllDriverAssignmentsLite(GetDriverAssignmentsInput input)
        {
            var query = _driverAssignmentRepository.GetAll()
                .Where(da => da.Date == input.Date && da.OfficeId == input.OfficeId && da.Truck.LocationId.HasValue)
                .WhereIf(input.Shift.HasValue && input.Shift != Shift.NoShift, da => da.Shift == input.Shift.Value)
                .WhereIf(input.Shift.HasValue && input.Shift == Shift.NoShift, da => da.Shift == null)
                .WhereIf(input.TruckId.HasValue, da => da.TruckId == input.TruckId);

            var items = await query
                .Select(x => new DriverAssignmentLiteDto
                {
                    Id = x.Id,
                    OfficeId = x.OfficeId,
                    Shift = x.Shift,
                    Date = x.Date,
                    TruckId = x.TruckId,
                    TruckCode = x.Truck.TruckCode,
                    DriverId = x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    DriverFirstName = x.Driver.FirstName,
                    DriverLastName = x.Driver.LastName,
                    DriverIsExternal = x.Driver.IsExternal == true,
                    DriverIsActive = x.Driver.IsInactive != true,
                    StartTime = x.StartTime,
                })
                .ToListAsync();

            var timezone = await GetTimezone();
            items.ForEach(x => x.StartTime = x.StartTime?.ConvertTimeZoneTo(timezone));

            return new ListResultDto<DriverAssignmentLiteDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        [HttpPost]
        public async Task<ListResultDto<DriverAssignmentDto>> GetDriverAssignments(GetDriverAssignmentsInput input)
        {
            var itemsLite = (await GetAllDriverAssignmentsLite(input)).Items;
            var totalCount = itemsLite.Count;

            var items = itemsLite.Select(x => x.CopyTo(new DriverAssignmentDto())).ToList();
            await FillFirstTimeOnJob(items, input);

            items = items
                .AsQueryable()
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToList();

            return new PagedResultDto<DriverAssignmentDto>(
                totalCount,
                items);
        }

        public async Task<SetNoDriverForTruckResult> SetNoDriverForTruck(SetNoDriverForTruckInput input)
        {
            if (input.StartDate < await GetToday())
            {
                throw new UserFriendlyException("Cannot set driver for date in past");
            }

            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;

            if (input.EndDate < input.StartDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            var sharedTruckResult = await _truckRepository.EnsureCanEditTruckOrSharedTruckAsync(input.TruckId, OfficeId, input.StartDate, input.EndDate);

            var existingAssignments = await _driverAssignmentRepository.GetAll()
                .Where(da => da.Date >= input.StartDate && da.Date <= input.EndDate && da.Shift == input.Shift && da.TruckId == input.TruckId)
                .ToListAsync();

            var today = await GetToday();
            var syncRequest = new SyncRequest();
            var result = new SetNoDriverForTruckResult();

            var dispatchesToCancel = await _dispatchRepository.GetAll()
                .Where(d => d.TruckId == input.TruckId
                    && d.OrderLine.Order.DeliveryDate >= input.StartDate
                    && d.OrderLine.Order.DeliveryDate <= input.EndDate
                    && d.OrderLine.Order.Shift == input.Shift
                    && Dispatch.UnacknowledgedStatuses.Contains(d.Status))
                .ToListAsync();
            dispatchesToCancel.ForEach(d =>
            {
                d.Status = DispatchStatus.Canceled;
                d.Canceled = Clock.Now;
            });
            await CurrentUnitOfWork.SaveChangesAsync();
            if (dispatchesToCancel.Any())
            {
                syncRequest
                    .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage("Canceled unacknowledged dispatches after no driver was set for the truck");
            }

            var driverIdsToNotify = new List<int>();
            var orderLineIdsNeedingStaggeredTimeRecalculation = new List<int>();

            var date = input.StartDate;
            while (date <= input.EndDate)
            {
                var existingAssignmentsForDay = existingAssignments.Where(x => x.Date == date && x.Shift == input.Shift);
                if (existingAssignmentsForDay.Any())
                {
                    var firstDriverAssignment = true;
                    foreach (var existingAssignment in existingAssignmentsForDay)
                    {
                        if (existingAssignment.DriverId.HasValue)
                        {
                            driverIdsToNotify.Add(existingAssignment.DriverId.Value);
                        }
                        var oldDriverId = existingAssignment.DriverId;
                        if (firstDriverAssignment)
                        {
                            existingAssignment.DriverId = null;
                            existingAssignment.OfficeId = sharedTruckResult.GetLocationForDate(date, input.Shift);
                        }
                        else
                        {
                            await _driverAssignmentRepository.DeleteAsync(existingAssignment);
                        }
                        syncRequest.AddChange(EntityEnum.DriverAssignment,
                            existingAssignment
                                .ToChangedEntity()
                                .SetOldDriverIdToNotify(oldDriverId),
                            changeType: firstDriverAssignment ? ChangeType.Modified : ChangeType.Removed);

                        firstDriverAssignment = false;
                    }
                }
                else
                {
                    var newDriverAssignment = new DriverAssignment
                    {
                        OfficeId = sharedTruckResult.GetLocationForDate(date, input.Shift),
                        Date = date,
                        Shift = input.Shift,
                        TruckId = input.TruckId,
                        DriverId = null
                    };
                    await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                    existingAssignments.Add(newDriverAssignment);
                    syncRequest.AddChange(EntityEnum.DriverAssignment,
                        newDriverAssignment
                            .ToChangedEntity());
                }

                var orderLineTrucksToDelete = await _orderLineTruckRepository.GetAll()
                    // ReSharper disable once AccessToModifiedClosure
                    .Where(olt => olt.TruckId == input.TruckId && olt.OrderLine.Order.DeliveryDate == date && olt.OrderLine.Order.Shift == input.Shift)
                    .ToListAsync();
                if (orderLineTrucksToDelete.Count > 0)
                {
                    result.TruckWasRemovedFromOrders = true;
                }
                foreach (var orderLineTruck in orderLineTrucksToDelete)
                {
                    _orderLineTruckRepository.Delete(orderLineTruck);
                    if (date >= today)
                    {
                        orderLineIdsNeedingStaggeredTimeRecalculation.Add(orderLineTruck.OrderLineId);
                    }
                }

                date = date.AddDays(1);
            }

            if (orderLineIdsNeedingStaggeredTimeRecalculation.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var orderLineId in orderLineIdsNeedingStaggeredTimeRecalculation.Distinct().ToList())
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                    orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                    await orderLineUpdater.SaveChangesAsync();
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(driverIdsToNotify)
            {
                LogMessage = $"Set no driver for truck"
            });
            await _syncRequestSender.SendSyncRequest(syncRequest.AddLogMessage("Set no driver for truck"));

            return result;
        }

        public async Task<ThereAreOpenDispatchesForTruckOnDateResult> ThereAreOpenDispatchesForTruckOnDate(ThereAreOpenDispatchesForTruckOnDateInput input)
        {
            var dispatchStatuses = await _dispatchRepository.GetAll()
                .Where(d => d.TruckId == input.TruckId && d.OrderLine.Order.DeliveryDate >= input.StartDate &&
                            d.OrderLine.Order.DeliveryDate <= input.EndDate && d.OrderLine.Order.Shift == input.Shift &&
                            !Dispatch.ClosedDispatchStatuses.Contains(d.Status))
                .GroupBy(d => d.Status)
                .Select(d => d.Key)
                .ToListAsync();
            return new ThereAreOpenDispatchesForTruckOnDateResult()
            {
                ThereAreUnacknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Created || ds == DispatchStatus.Sent),
                ThereAreAcknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Acknowledged || ds == DispatchStatus.Loaded),
            };
        }

        public async Task<ThereAreOpenDispatchesForDriverOnDateResult> ThereAreOpenDispatchesForDriverOnDate(ThereAreOpenDispatchesForDriverOnDateInput input)
        {
            var dispatchStatuses = await _dispatchRepository.GetAll()
                .Where(d => d.DriverId == input.DriverId && d.OrderLine.Order.DeliveryDate >= input.StartDate &&
                            d.OrderLine.Order.DeliveryDate <= input.EndDate &&
                            !Dispatch.ClosedDispatchStatuses.Contains(d.Status))
                .GroupBy(d => d.Status)
                .Select(d => d.Key)
                .ToListAsync();
            return new ThereAreOpenDispatchesForDriverOnDateResult()
            {
                ThereAreUnacknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Created || ds == DispatchStatus.Sent),
                ThereAreAcknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Acknowledged || ds == DispatchStatus.Loaded),
            };
        }

        private async Task ThrowIfDriverHasTimeOffRequests(int driverId, DateTime startDate, DateTime endDate)
        {
            if (await _timeOffRepository.GetAll()
                    .AnyAsync(x => x.DriverId == driverId && startDate <= x.EndDate && endDate >= x.StartDate))
            {
                throw new UserFriendlyException(L("DriverCantBeAssignedOnDayOff"));
            }
        }

        public async Task/*<SetDriverForTruckResult>*/ SetDriverForTruck(SetDriverForTruckInput input)
        {
            await EnsureCanAssignDriverToTruck(input.TruckId);
            if (input.DriverId != null)
            {
                await ThrowIfDriverHasTimeOffRequests(input.DriverId.Value, input.Date, input.Date);
            }
            var sharedTruckResult = await _truckRepository.EnsureCanEditTruckOrSharedTruckAsync(input.TruckId, OfficeId, input.Date);
            var officeIdForDriverAssignment = sharedTruckResult.GetLocationForDate(input.Date, input.Shift) ?? input.OfficeId ?? throw new UserFriendlyException("You need to select the office first");

            var driverAssignments = await GetAllDriverAssignmentsLite(new GetDriverAssignmentsInput()
            {
                Date = input.Date,
                OfficeId = officeIdForDriverAssignment,
                Shift = input.Shift,
                TruckId = input.TruckId,
            });

            var driverAssignment = driverAssignments.Items.OrderBy(x => x.Id).LastOrDefault();
            if (driverAssignment == null)
            {
                input.CreateNewDriverAssignment = true;
            }

            await EditDriverAssignment(new DriverAssignmentEditDto
            {
                Id = !input.CreateNewDriverAssignment ? driverAssignment.Id : 0,
                Date = input.Date,
                Shift = input.Shift,
                OfficeId = officeIdForDriverAssignment,
                DriverId = input.DriverId,
                TruckId = input.TruckId,
                StartTime = !input.CreateNewDriverAssignment ? driverAssignment.StartTime : null
            });

            //if (await OrderLineTruckExists(new OrderLineTruckExistsInput(input.TruckId, input.Date, input.Shift)))
            //{
            //    return new SetDriverForTruckResult
            //    {
            //        Success = false,
            //        OrderLineTruckExists = true
            //    };
            //}
            //
            //var driverAssignment = await _driverAssignmentRepository.GetAll()
            //    .Where(da => da.TruckId == input.TruckId && da.Date == input.Date && da.Shift == input.Shift)
            //    .OrderByDescending(x => x.Id)
            //    .FirstOrDefaultAsync();
            //var driverIdsToNotify = new List<int>() { input.DriverId };
            //var syncRequest = new SyncRequest();
            //if (driverAssignment == null)
            //{
            //    DriverAssignment newDriverAssignment = new DriverAssignment
            //    {
            //        Date = input.Date,
            //        Shift = input.Shift,
            //        OfficeId = sharedTruckResult.GetLocationForDate(input.Date, input.Shift),
            //        TruckId = input.TruckId,
            //        DriverId = input.DriverId,
            //    };
            //    await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
            //    syncRequest.AddChange(EntityEnum.DriverAssignment, newDriverAssignment.ToChangedEntity());
            //}
            //else
            //{
            //    if (driverAssignment.DriverId.HasValue)
            //    {
            //        driverIdsToNotify.Add(driverAssignment.DriverId.Value);
            //    }
            //    var oldDriverId = driverAssignment.DriverId;
            //    driverAssignment.DriverId = input.DriverId;
            //    driverAssignment.OfficeId = sharedTruckResult.GetLocationForDate(input.Date, input.Shift);
            //    syncRequest.AddChange(EntityEnum.DriverAssignment, driverAssignment
            //        .ToChangedEntity()
            //        .SetOldDriverIdToNotify(oldDriverId));
            //}
            //
            //await CurrentUnitOfWork.SaveChangesAsync();
            //await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(driverIdsToNotify)
            //{
            //    LogMessage = $"Set driver for truck"
            //});
            //await _syncRequestSender.SendSyncRequest(syncRequest
            //    .AddLogMessage("Set driver for truck"));
            //
            //return new SetDriverForTruckResult
            //{
            //    Success = true
            //};
        }

        private async Task EnsureCanAssignDriverToTruck(int truckId)
        {
            var vehicleCategoryIsPowered = await _truckRepository.GetAll().Where(t => t.Id == truckId).Select(t => t.VehicleCategory.IsPowered).FirstAsync();
            if (!vehicleCategoryIsPowered)
            {
                throw new ArgumentException("Cannot set driver for an unpowered truck!");
            }
        }

        public async Task SetDefaultDriverForTruck(SetDefaultDriverForTruckInput input)
        {
            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;

            var sharedTruckResult = await _truckRepository.EnsureCanEditTruckOrSharedTruckAsync(input.TruckId, OfficeId, input.StartDate, input.EndDate);

            if (input.EndDate < input.StartDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            int? defaultDriverId = await _truckRepository.GetAll()
                .Where(t => t.Id == input.TruckId)
                .Select(t => t.DefaultDriverId)
                .FirstAsync();
            if (!defaultDriverId.HasValue)
            {
                throw new ArgumentException("The truck has no default driver!");
            }

            await ThrowIfDriverHasTimeOffRequests(defaultDriverId.Value, input.StartDate, input.EndDate);

            var syncRequest = new SyncRequest();
            var driverIdsToNotify = new List<int>();
            driverIdsToNotify.Add(defaultDriverId.Value);
            var existingAssignments = await _driverAssignmentRepository.GetAll()
                .Where(x => x.Date >= input.StartDate && x.Date <= input.EndDate && x.Shift == input.Shift && x.TruckId == input.TruckId)
                .ToListAsync();

            foreach (var dayGroup in existingAssignments.GroupBy(x => x.Date))
            {
                var firstAssignment = true;
                foreach (var driverAssignment in dayGroup)
                {
                    var oldDriverId = driverAssignment.DriverId;
                    if (!firstAssignment)
                    {
                        if (oldDriverId.HasValue)
                        {
                            driverIdsToNotify.Add(oldDriverId.Value);
                        }
                        syncRequest
                            .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(oldDriverId), ChangeType.Removed);

                        await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                    }
                    else
                    {
                        if (driverAssignment.DriverId.HasValue && driverAssignment.DriverId != defaultDriverId.Value)
                        {
                            driverIdsToNotify.Add(driverAssignment.DriverId.Value);
                            driverAssignment.DriverId = defaultDriverId.Value;
                            syncRequest
                                .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(oldDriverId));
                        }
                        else
                        {
                            driverAssignment.DriverId = defaultDriverId.Value;
                        }
                    }

                    firstAssignment = false;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(driverIdsToNotify)
            {
                LogMessage = $"Set default driver for truck"
            });
            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Set default driver for truck"));
        }

        public async Task<bool> OrderLineTruckExists(OrderLineTruckExistsInput input) =>
            await _orderLineTruckRepository.GetAll()
                .AnyAsync(olt => olt.TruckId == input.TruckId && olt.OrderLine.Order.DeliveryDate == input.Date && olt.OrderLine.Order.Shift == input.Shift && !olt.IsDone);

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<byte[]> GetDriverAssignmentReport(GetDriverAssignmentsInput input)
        {
            Shift? shift = input.Shift == Shift.NoShift ? null : input.Shift;
            var items = await _driverAssignmentRepository.GetAll(input.Date, shift, input.OfficeId)
                .Where(da => da.Truck.LocationId.HasValue)
                .Select(da => new DriverAssignmentReportItemDto
                {
                    Id = da.Id,
                    TruckId = da.TruckId,
                    TruckCode = da.Truck.TruckCode,
                    DriverId = da.DriverId,
                    DriverName = da.Driver.FirstName + " " + da.Driver.LastName,
                    DriverFirstName = da.Driver.FirstName,
                    DriverLastName = da.Driver.LastName,
                    DriverIsExternal = da.Driver.IsExternal == true,
                    DriverIsActive = da.Driver.IsInactive != true,
                    StartTime = da.StartTime,
                    OfficeName = da.Office.Name
                })
                .OrderBy(x => x.TruckCode)
                .ToListAsync();

            var timezone = await GetTimezone();
            items.ForEach(x => x.StartTime = x.StartTime?.ConvertTimeZoneTo(timezone));

            await FillFirstTimeOnJob(items, input);

            var data = new DriverAssignmentReportDto
            {
                Date = input.Date,
                Shift = input.Shift,
                ShiftName = await SettingManager.GetShiftName(input.Shift),
                Items = items,
            };

            return DriverAssignmentReportGenerator.GenerateReport(data);
        }

        public async Task ChangeDriverForOrderLineTruck(ChangeDriverForOrderLineTruckInput input)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAll()
                .Where(x => x.Id == input.OrderLineTruckId)
                .FirstAsync();

            var orderDetails = await _orderLineTruckRepository.GetAll()
                .Where(x => x.Id == input.OrderLineTruckId)
                .Select(x => new
                {
                    IsExternalTruck = x.Truck.LocationId == null,
                    x.OrderLine.Order.DeliveryDate,
                    x.OrderLine.Order.Shift,
                    OfficeId = x.OrderLine.Order.LocationId
                }).FirstAsync();

            if (!orderDetails.DeliveryDate.HasValue)
            {
                throw new UserFriendlyException("Unavailable for pending orders without a set Delivery Date. Please set the order Delivery Date first");
            }
            await ThrowIfDriverHasTimeOffRequests(input.DriverId, orderDetails.DeliveryDate.Value, orderDetails.DeliveryDate.Value);

            int? oldDriverId = null;
            DriverAssignment driverAssignment = null;

            if (input.ReplaceExistingDriver)
            {
                if (orderDetails.IsExternalTruck)
                {
                    var availableLeaseHaulerTrucks = await _availableLeaseHaulerTruckRepository.GetAll()
                        .Where(x => x.Date == orderDetails.DeliveryDate.Value
                            && x.OfficeId == orderDetails.OfficeId
                            && x.Shift == orderDetails.Shift
                            && x.TruckId == orderLineTruck.TruckId)
                        .ToListAsync();

                    foreach (var availableLeaseHaulerTruck in availableLeaseHaulerTrucks)
                    {
                        availableLeaseHaulerTruck.DriverId = input.DriverId;
                    }
                }
                else
                {
                    driverAssignment = await _driverAssignmentRepository.GetAll(orderDetails.DeliveryDate.Value, orderDetails.Shift, orderDetails.OfficeId)
                        .Where(x => x.TruckId == orderLineTruck.TruckId && x.DriverId == orderLineTruck.DriverId)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync();

                    if (driverAssignment != null)
                    {
                        oldDriverId = driverAssignment.DriverId;
                        //driverAssignment.DriverId = input.DriverId;
                        await EditDriverAssignment(new DriverAssignmentEditDto()
                        {
                            Id = driverAssignment.Id,
                            TruckId = driverAssignment.TruckId,
                            DriverId = input.DriverId,
                            StartTime = driverAssignment.StartTime,
                            OfficeId = driverAssignment.OfficeId,
                            Date = driverAssignment.Date,
                            Shift = driverAssignment.Shift,
                        });
                    }
                    else
                    {
                        //handled below
                    }
                }
            }
            else
            {
                //handled below
            }

            //if !ReplaceExistingDriver or if ReplaceExistingDriver but we didn't find a matching DriverAssignment - create a new driver assignment
            if (driverAssignment == null && !orderDetails.IsExternalTruck)
            {
                if (!await _driverAssignmentRepository.GetAll(orderDetails.DeliveryDate.Value, orderDetails.Shift, orderDetails.OfficeId)
                    .AnyAsync(x => x.TruckId == orderLineTruck.TruckId && x.DriverId == input.DriverId))
                {
                    var sharedTruckResult = await _truckRepository.EnsureCanEditTruckOrSharedTruckAsync(orderLineTruck.TruckId, OfficeId, orderDetails.DeliveryDate.Value);

                    await EditDriverAssignment(new DriverAssignmentEditDto()
                    {
                        Id = 0,
                        Date = orderDetails.DeliveryDate.Value,
                        Shift = orderDetails.Shift,
                        OfficeId = sharedTruckResult.GetLocationForDate(orderDetails.DeliveryDate.Value, orderDetails.Shift),
                        TruckId = orderLineTruck.TruckId,
                        DriverId = input.DriverId,
                    });
                }
            }

            orderLineTruck.DriverId = input.DriverId;


            //await CurrentUnitOfWork.SaveChangesAsync();
            //if (driverAssignment != null)
            //{
            //    var logMessage = $"Changed or created driver assignment for truck {driverAssignment.TruckId} from driver {oldDriverId?.ToString() ?? "null"} to {input.DriverId}";
            //    if (oldDriverId.HasValue && oldDriverId != driverAssignment.DriverId)
            //    {
            //        await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(oldDriverId.Value)
            //        {
            //            LogMessage = logMessage
            //        });
            //    }
            //    if (input.DriverId != oldDriverId)
            //    {
            //        await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(input.DriverId)
            //        {
            //            LogMessage = logMessage
            //        });
            //    }
            //    await _syncRequestSender.SendSyncRequest(new SyncRequest()
            //        .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(oldDriverId))
            //        .AddLogMessage(logMessage));
            //}
            //
            //await RemoveDuplicateDriverAssignments(new RemoveDuplicateDriverAssignmentsInput
            //{
            //    Date = orderDetails.DeliveryDate.Value,
            //    Shift = orderDetails.Shift,
            //    OfficeId = orderDetails.OfficeId,
            //    TruckId = orderLineTruck.TruckId,
            //});
        }

        private async Task RemoveDuplicateDriverAssignments(RemoveDuplicateDriverAssignmentsInput input)
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            var driverAssignments = await _driverAssignmentRepository.GetAll(input.Date, input.Shift, input.OfficeId)
                .Where(x => x.TruckId == input.TruckId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            if (driverAssignments.Count <= 1)
            {
                return;
            }

            if (driverAssignments[0].DriverId == null)
            {
                foreach (var driverAssignment in driverAssignments.Skip(1))
                {
                    await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                }
                return;
            }

            if (driverAssignments.Any(x => x.DriverId == null) && driverAssignments.Any(x => x.DriverId != null))
            {
                foreach (var driverAssignment in driverAssignments.Where(x => x.DriverId == null).ToList())
                {
                    await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                    driverAssignments.Remove(driverAssignment);
                }
            }

            foreach (var driverAssignmentGroup in driverAssignments.GroupBy(x => x.DriverId))
            {
                foreach (var driverAssignment in driverAssignmentGroup.Skip(1))
                {
                    await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                }
            }
        }

        private async Task FillFirstTimeOnJob(IEnumerable<DriverAssignmentDto> items, GetDriverAssignmentsInput input)
        {
            var timesOnJobRaw = await _orderLineTruckRepository.GetAll()
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date && x.OrderLine.Order.Shift == input.Shift && x.OrderLine.Order.LocationId == input.OfficeId)
                .Select(x => new
                {
                    x.TruckId,
                    x.DriverId,
                    OrderLineTimeOnJobUtc = x.OrderLine.TimeOnJob,
                    TimeOnJobUtc = x.TimeOnJob,
                    LoadAt = x.OrderLine.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = x.OrderLine.LoadAt.Name,
                        StreetAddress = x.OrderLine.LoadAt.StreetAddress,
                        City = x.OrderLine.LoadAt.City,
                        State = x.OrderLine.LoadAt.State
                    },
                }).ToListAsync();
            var timesOnJob = timesOnJobRaw.GroupBy(x => new { x.TruckId, x.DriverId });

            var timezone = await GetTimezone();
            foreach (var item in items)
            {
                var timeOnJobToUse = timesOnJob
                    .FirstOrDefault(t => t.Key.TruckId == item.TruckId && t.Key.DriverId == item.DriverId)
                    ?.OrderBy(t => t.TimeOnJobUtc ?? t.OrderLineTimeOnJobUtc)
                    .FirstOrDefault();
                item.FirstTimeOnJob = (timeOnJobToUse?.TimeOnJobUtc ?? timeOnJobToUse?.OrderLineTimeOnJobUtc)?.ConvertTimeZoneTo(timezone);
                item.LoadAtName = timeOnJobToUse?.LoadAt?.FormattedAddress;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<int> AddUnscheduledTrucks(AddUnscheduledTrucksInput input)
        {
            var unscheduledTrucks = await _truckRepository.GetAll()
                .Where(t =>
                    (t.LocationId == input.OfficeId && !t.SharedTrucks.Any(s => s.Date == input.Date && s.Shift == input.Shift)
                        || t.SharedTrucks.Any(s => s.Date == input.Date && s.OfficeId == input.OfficeId && s.Shift == input.Shift)) &&
                    t.VehicleCategory.IsPowered && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true && t.LocationId != null &&
                    !t.DriverAssignments.Any(da => da.Date == input.Date && da.Shift == input.Shift) &&
                    t.IsActive &&
                    !t.IsOutOfService
                )
                .Select(t => new
                {
                    t.Id,
                    t.DefaultDriverId,
                })
                .ToListAsync();

            var timezone = await GetTimezone();

            var syncRequest = new SyncRequest();
            var driverIdsToNotify = new List<int>();
            foreach (var unscheduledTruck in unscheduledTrucks)
            {
                DriverAssignment driverAssignment = new DriverAssignment()
                {
                    OfficeId = input.OfficeId,
                    Date = input.Date,
                    Shift = input.Shift,
                    TruckId = unscheduledTruck.Id,
                    DriverId = input.Shift == null || input.Shift == Shift.Shift1 ? unscheduledTruck.DefaultDriverId : null,
                    StartTime = input.DefaultStartTime.HasValue
                        ? input.Date.Add(input.DefaultStartTime.Value.TimeOfDay).ConvertTimeZoneFrom(timezone)
                        : (DateTime?)null
                };
                if (driverAssignment.DriverId.HasValue)
                {
                    driverIdsToNotify.Add(driverAssignment.DriverId.Value);
                }
                syncRequest.AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity());
                await _driverAssignmentRepository.InsertAsync(driverAssignment);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(driverIdsToNotify)
            {
                LogMessage = $"Added driver assignments for unscheduled trucks"
            });
            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Added driver assignments for unscheduled trucks"));

            return unscheduledTrucks.Count;
        }

        public async Task AddDefaultDriverAssignments(AddDefaultDriverAssignmentsInput input)
        {
            var unscheduledDrivers = await _driverRepository.GetAll()
                .Where(d =>
                    !d.IsExternal && d.OfficeId == input.OfficeId && !d.TimeOffs.Any(to => to.StartDate <= input.Date && to.EndDate >= input.Date) &&
                    !d.DriverAssignments.Any(da => da.Date == input.Date && da.Shift == input.Shift) &&
                    !d.IsInactive &&
                    d.DefaultTrucks.Any(t => t.LocationId != null && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true)
                )
                .Select(d => new
                {
                    d.Id,
                    DefaultTruckId = (int?)d.DefaultTrucks.FirstOrDefault(t => t.LocationId != null && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true).Id,
                })
                .ToListAsync();

            unscheduledDrivers.RemoveAll(x => x.DefaultTruckId == null);

            var syncRequest = new SyncRequest();
            foreach (var unscheduledDriver in unscheduledDrivers)
            {
                var driverAssignment = new DriverAssignment
                {
                    OfficeId = input.OfficeId,
                    Date = input.Date,
                    Shift = input.Shift,
                    TruckId = unscheduledDriver.DefaultTruckId.Value,
                    DriverId = unscheduledDriver.Id,
                    StartTime = null
                };
                syncRequest.AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity());
                await _driverAssignmentRepository.InsertAsync(driverAssignment);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(unscheduledDrivers.Select(x => x.Id))
            {
                LogMessage = $"Added default driver assignments"
            });
            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Added default driver assignments"));
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<EditDriverAssignmentResult> EditDriverAssignment(DriverAssignmentEditDto input)
        {
            var result = new EditDriverAssignmentResult();

            var driverAssignment = input.Id > 0 ? await _driverAssignmentRepository.GetAsync(input.Id) : new DriverAssignment();
            if (driverAssignment.Id == 0)
            {
                driverAssignment.OfficeId = input.OfficeId;
                driverAssignment.Date = input.Date;
                driverAssignment.Shift = input.Shift;
                driverAssignment.TruckId = input.TruckId;
                await _driverAssignmentRepository.InsertAndGetIdAsync(driverAssignment);
            }

            var logMessage = "";
            var oldDriverId = driverAssignment.DriverId;
            var driverAssignmentWasChanged = false;
            var driverAssignmentWasDeleted = false;

            if (driverAssignment.DriverId != input.DriverId)
            {
                if (input.DriverId.HasValue)
                {
                    await ThrowIfDriverHasTimeOffRequests(input.DriverId.Value, driverAssignment.Date, driverAssignment.Date);
                }
                var updateOrderLineTrucks = false;
                if (input.Id > 0 && oldDriverId.HasValue)
                {
                    var validationResult = await HasOrderLineTrucks(new HasOrderLineTrucksInput
                    {
                        Date = driverAssignment.Date,
                        OfficeId = driverAssignment.OfficeId,
                        Shift = driverAssignment.Shift,
                        TruckId = driverAssignment.TruckId,
                        DriverId = oldDriverId,
                    });
                    if (validationResult.HasOpenDispatches)
                    {
                        throw new UserFriendlyException(L("CannotChangeDriverBecauseOfDispatchesError"));
                    }
                    if (validationResult.HasOrderLineTrucks)
                    {
                        if (input.DriverId == null)
                        {
                            throw new UserFriendlyException(L("CannotRemoveDriverBecauseOfOrderLineTrucksError"));
                        }

                        updateOrderLineTrucks = true;
                    }
                }
                else if (oldDriverId == null)
                {
                    updateOrderLineTrucks = true;
                }

                if (updateOrderLineTrucks)
                {
                    var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                            .Where(x => driverAssignment.Date == x.OrderLine.Order.DeliveryDate && driverAssignment.Shift == x.OrderLine.Order.Shift)
                            .WhereIf(driverAssignment.OfficeId.HasValue, x => driverAssignment.OfficeId == x.OrderLine.Order.LocationId)
                            .Where(x => oldDriverId == x.DriverId)
                            .Where(x => driverAssignment.TruckId == x.TruckId)
                            .ToListAsync();

                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.DriverId = input.DriverId;
                    }
                }

                driverAssignment.DriverId = input.DriverId;

                var sameTruckDriverAssignments = await _driverAssignmentRepository.GetAll()
                    .Where(x => x.Shift == driverAssignment.Shift
                        && x.OfficeId == driverAssignment.OfficeId
                        && x.Date == driverAssignment.Date
                        && x.TruckId == driverAssignment.TruckId
                        && x.Id != driverAssignment.Id)
                    .ToListAsync();

                var duplicateDriverAssignments = sameTruckDriverAssignments
                    .Where(x => x.DriverId == driverAssignment.DriverId)
                    .ToList();

                if (duplicateDriverAssignments.Any())
                {
                    foreach (var duplicateDriverAssignment in duplicateDriverAssignments)
                    {
                        await _driverAssignmentRepository.DeleteAsync(duplicateDriverAssignment);
                        sameTruckDriverAssignments.Remove(duplicateDriverAssignment);
                    }
                    result.ReloadRequired = true;
                }

                if (input.DriverId == null)
                {
                    if (input.Id > 0)
                    {
                        await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                        driverAssignmentWasDeleted = true;
                        result.ReloadRequired = true;
                    }
                    else
                    {
                        foreach (var sameTruckDriverAssignment in sameTruckDriverAssignments.ToList())
                        {
                            await _driverAssignmentRepository.DeleteAsync(sameTruckDriverAssignment);
                            sameTruckDriverAssignments.Remove(sameTruckDriverAssignment);
                            result.ReloadRequired = true;
                        }
                    }
                }

                logMessage += $"Changed driver assignment for truck {driverAssignment.TruckId} from driver {oldDriverId?.ToString() ?? "null"} to {input.DriverId?.ToString() ?? "null"}\n";
                driverAssignmentWasChanged = true;
            }

            if (driverAssignment.StartTime != input.StartTime)
            {
                if (input.StartTime.HasValue)
                {
                    input.StartTime = driverAssignment.Date.Add(input.StartTime.Value.TimeOfDay);
                }

                driverAssignment.StartTime = input.StartTime?.ConvertTimeZoneFrom(await GetTimezone());

                logMessage += $"Updated start time to {input.StartTime?.ToShortTimeString()} for driver assignment {driverAssignment.Id}\n";
                driverAssignmentWasChanged = true;
            }

            if (driverAssignmentWasChanged)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                if (oldDriverId.HasValue && (oldDriverId != input.DriverId || driverAssignmentWasDeleted))
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(oldDriverId.Value)
                    {
                        LogMessage = logMessage
                    });
                }
                if (!driverAssignmentWasDeleted && driverAssignment.DriverId.HasValue && driverAssignmentWasChanged)
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(driverAssignment.DriverId.Value)
                    {
                        LogMessage = logMessage
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(oldDriverId), driverAssignmentWasDeleted ? ChangeType.Removed : ChangeType.Modified)
                    .AddLogMessage(logMessage));
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<HasOrderLineTrucksResult> HasOrderLineTrucks(HasOrderLineTrucksInput input)
        {
            var result = new HasOrderLineTrucksResult();
            result.HasOrderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(x => input.Date == x.OrderLine.Order.DeliveryDate && input.Shift == x.OrderLine.Order.Shift)
                .WhereIf(input.OfficeId.HasValue, x => input.OfficeId == x.OrderLine.Order.LocationId)
                .WhereIf(input.DriverId.HasValue, x => input.DriverId == x.DriverId)
                .WhereIf(input.TrailerId.HasValue || input.ForceTrailerIdFilter, x => input.TrailerId == x.TrailerId)
                .WhereIf(input.TruckId.HasValue, x => input.TruckId == x.TruckId)
                .AnyAsync();

            result.HasOpenDispatches = await _dispatchRepository.GetAll()
                .Where(x => input.Date == x.OrderLine.Order.DeliveryDate
                    && input.Shift == x.OrderLine.Order.Shift
                    && Dispatch.OpenStatuses.Contains(x.Status))
                .WhereIf(input.OfficeId.HasValue, x => input.OfficeId == x.OrderLine.Order.LocationId)
                .WhereIf(input.DriverId.HasValue, x => input.DriverId == x.DriverId)
                .WhereIf(input.TrailerId.HasValue, x => input.TrailerId == x.OrderLineTruck.TrailerId)
                .WhereIf(input.TruckId.HasValue, x => input.TruckId == x.TruckId)
                .AnyAsync();

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task AddDefaultStartTime(AddDefaultStartTimeInput input)
        {
            if (!input.DefaultStartTime.HasValue)
            {
                throw new UserFriendlyException("Default Start Time is required");
            }

            var driverAssignments = await _driverAssignmentRepository
                .GetAll(input.Date, input.Shift, input.OfficeId)
                .Where(x => x.StartTime == null)
                .ToListAsync();

            if (!driverAssignments.Any())
            {
                return;
            }

            var timezone = await GetTimezone();
            driverAssignments.ForEach(x => x.StartTime = x.Date.Add(input.DefaultStartTime.Value.TimeOfDay).ConvertTimeZoneFrom(timezone));
            var driverIdsToNotify = driverAssignments.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value).Distinct().ToList();

            await CurrentUnitOfWork.SaveChangesAsync();
            if (driverIdsToNotify.Any())
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(driverIdsToNotify)
                {
                    LogMessage = $"Added scheduled start time"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.DriverAssignment, driverAssignments.Select(x => x.ToChangedEntity()))
                .AddLogMessage("Added scheduled start time"));
        }
    }
}
