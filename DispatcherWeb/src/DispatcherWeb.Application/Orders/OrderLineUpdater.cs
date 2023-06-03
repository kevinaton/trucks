using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Infrastructure.EntityUpdaters;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.SyncRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders
{
    public class OrderLineUpdater : EntityUpdater<OrderLine>, IOrderLineUpdater
    {
        private readonly int _orderLineId;
        private readonly int[] _alreadySyncedOrderLineIds;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly IReadonlyCheckerFactory<OrderLine> _orderLineReadonlyCheckerFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAbpSession _abpSession;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IOrderLineScheduledTrucksUpdater _orderLineScheduledTrucksUpdater;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;

        private bool _needToRecalculateFuelSurcharge = false;
        private List<string> _updatedFields = new List<string>();

        public OrderLineUpdater(
            int orderLineId,
            int[] alreadySyncedOrderLineIds,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            IReadonlyCheckerFactory<OrderLine> orderLineReadonlyCheckerFactory,
            ILocalizationManager localizationManager,
            IUnitOfWorkManager unitOfWorkManager,
            IAbpSession abpSession,
            IDriverApplicationPushSender driverApplicationPushSender,
            ISyncRequestSender syncRequestSender,
            IOrderLineScheduledTrucksUpdater orderLineScheduledTrucksUpdater,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Order> orderRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            ICrossTenantOrderSender crossTenantOrderSender
            ) : base(
                localizationManager
                )
        {
            _orderLineId = orderLineId;
            _alreadySyncedOrderLineIds = alreadySyncedOrderLineIds;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _orderLineReadonlyCheckerFactory = orderLineReadonlyCheckerFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _abpSession = abpSession;
            _driverApplicationPushSender = driverApplicationPushSender;
            _syncRequestSender = syncRequestSender;
            _orderLineScheduledTrucksUpdater = orderLineScheduledTrucksUpdater;
            _orderLineRepository = orderLineRepository;
            _orderRepository = orderRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _crossTenantOrderSender = crossTenantOrderSender;
        }

        private OrderLine _orderLine = null;
        public override async Task<OrderLine> GetEntityAsync()
        {
            return _orderLine ?? (_orderLine = _orderLineId == 0 ? new OrderLine() : await _orderLineRepository.GetAsync(_orderLineId));
        }

        private Order _order = null;
        public async Task<Order> GetOrderAsync()
        {
            var orderLine = await GetEntityAsync();
            if (orderLine.OrderId == 0)
            {
                throw new Exception("orderLine.OrderId is not set");
            }
            return _order ?? (_order = await _orderRepository.GetAsync(orderLine.OrderId));
        }

        private bool _suppressReadOnlyChecker = false;
        public void SuppressReadOnlyChecker(bool suppressReadOnlyChecker = true)
        {
            _suppressReadOnlyChecker = suppressReadOnlyChecker;
        }

        protected override async Task UpdateFieldAsync<TField>(OrderLine orderLine, string fieldName, TField oldValue, TField newValue, Action<TField> setValue)
        {
            if (oldValue is string)
            {
                if ((oldValue as string ?? "") == (newValue as string ?? ""))
                {
                    return;
                }
            }
            else if (EqualityComparer<TField>.Default.Equals(oldValue, newValue))
            {
                return;
            }

            if (!_suppressReadOnlyChecker)
            {
                await GetReadonlyChecker(orderLine).ThrowIfFieldIsReadonlyAsync(fieldName);
            }

            setValue(newValue);

            _updatedFields.Add(fieldName);

            switch (fieldName)
            {
                case nameof(orderLine.Note):
                    await ForEachDispatchWhereAsync(
                        d => d.Status != DispatchStatus.Completed && d.Note != orderLine.Note,
                        d => d.Note = orderLine.Note);
                    break;
                case nameof(orderLine.LoadAtId):
                    await MarkAffectedDispatchesWhereAsync(d => !d.Status.IsIn(DispatchStatus.Completed, DispatchStatus.Loaded));
                    break;
                case nameof(orderLine.DeliverToId):
                case nameof(orderLine.ServiceId):
                case nameof(orderLine.Designation):
                case nameof(orderLine.FreightQuantity):
                case nameof(orderLine.MaterialQuantity):
                case nameof(orderLine.MaterialUomId):
                case nameof(orderLine.FreightUomId):
                case nameof(orderLine.ProductionPay):
                case nameof(orderLine.JobNumber):
                    await MarkAffectedDispatchesWhereAsync(d => !d.Status.IsIn(DispatchStatus.Completed));
                    break;
            }

            switch (fieldName)
            {
                case nameof(orderLine.IsFreightPriceOverridden):
                case nameof(orderLine.IsMaterialPriceOverridden):
                case nameof(orderLine.IsMultipleLoads):
                    if (orderLine.IsFreightPriceOverridden || orderLine.IsMaterialPriceOverridden)
                    {
                        _needToValidateOverriddenTotals = true;
                    }
                    break;
            }

            switch (fieldName)
            {
                case nameof(orderLine.FreightQuantity):
                case nameof(orderLine.FreightPricePerUnit):
                case nameof(orderLine.FreightPrice):
                case nameof(orderLine.IsFreightPriceOverridden):
                    _needToRecalculateFuelSurcharge = true;
                    break;
            }

            switch (fieldName)
            {
                case nameof(orderLine.StaggeredTimeKind):
                case nameof(orderLine.StaggeredTimeInterval):
                case nameof(orderLine.FirstStaggeredTimeOnJob):
                    _needToUpdateStaggeredTimeOnTrucks = true;
                    break;
            }
        }

        private List<Dispatch> _dispatches = null;
        private List<Dispatch> _affectedDispatches = new List<Dispatch>();
        private async Task<List<Dispatch>> GetDispatchesAsync()
        {
            return _dispatches ?? (_orderLineId == 0 ? new List<Dispatch>() : await _dispatchRepository.GetAll().Where(x => x.OrderLineId == _orderLineId).ToListAsync());
        }

        private async Task ForEachDispatchAsync(Action<Dispatch> action)
        {
            var dispatches = await GetDispatchesAsync();
            foreach (var dispatch in dispatches)
            {
                action(dispatch);
                if (!_affectedDispatches.Contains(dispatch))
                {
                    _affectedDispatches.Add(dispatch);
                }
            }
        }

        private async Task ForEachDispatchWhereAsync(Func<Dispatch, bool> wherePredicate, Action<Dispatch> action)
        {
            var dispatches = await GetDispatchesAsync();
            foreach (var dispatch in dispatches)
            {
                if (wherePredicate(dispatch))
                {
                    action(dispatch);
                    if (!_affectedDispatches.Contains(dispatch))
                    {
                        _affectedDispatches.Add(dispatch);
                    }
                }
            }
        }

        public async Task MarkAffectedDispatchesWhereAsync(Func<Dispatch, bool> wherePredicate)
        {
            var dispatches = await GetDispatchesAsync();
            foreach (var dispatch in dispatches)
            {
                if (wherePredicate(dispatch))
                {
                    if (!_affectedDispatches.Contains(dispatch))
                    {
                        _affectedDispatches.Add(dispatch);
                    }
                }
            }
        }

        private bool _needToValidateOverriddenTotals = false;
        private async Task ValidateOverriddenTotalsIfNeeded(OrderLine orderLine)
        {
            if (!_needToValidateOverriddenTotals)
            {
                return;
            }
            _needToValidateOverriddenTotals = false;

            if (!orderLine.IsFreightPriceOverridden && !orderLine.IsMaterialPriceOverridden)
            {
                return;
            }

            if (orderLine.IsMultipleLoads)
            {
                throw new UserFriendlyException(L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"));
            }

            if (orderLine.Id == 0)
            {
                return;
            }

            var orderLineData = await _orderLineRepository.GetAll()
                .Where(x => x.Id == _orderLineId)
                .Select(x => new
                {
                    HasMultipleTickets = x.Tickets.Count() > 1,
                    HasMultipleTrucks = x.OrderLineTrucks.Count(x => x.Truck.VehicleCategory.IsPowered) > 1,
                    HasMultipleDispatches = x.Dispatches.Count(x => x.Status != DispatchStatus.Canceled) > 1,
                }).FirstAsync();

            var hasMultipleTicketsOrDispatchesOrTrucks = orderLineData.HasMultipleTickets
                || orderLineData.HasMultipleTrucks
                || orderLineData.HasMultipleDispatches;

            if (hasMultipleTicketsOrDispatchesOrTrucks)
            {
                throw new UserFriendlyException(L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"));
            }
        }

        public void UpdateStaggeredTimeOnTrucksOnSave()
        {
            _needToUpdateStaggeredTimeOnTrucks = true;
        }

        private bool _needToUpdateStaggeredTimeOnTrucks = false;
        private async Task UpdateStaggeredTimeOnTrucks()
        {
            if (_orderLineId == 0)
            {
                return;
            }

            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(x => x.OrderLineId == _orderLineId)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var orderLine = await GetEntityAsync();

            var affectedTruckIds = new List<int>();

            if (orderLine.StaggeredTimeKind == StaggeredTimeKind.None)
            {
                foreach (var orderLineTruck in orderLineTrucks)
                {
                    if (orderLineTruck.TimeOnJob != null)
                    {
                        orderLineTruck.TimeOnJob = null;
                        affectedTruckIds.Add(orderLineTruck.TruckId);
                    }
                }
            }
            else if (orderLine.StaggeredTimeKind == StaggeredTimeKind.SetInterval)
            {
                var truckData = await _orderLineTruckRepository.GetAll()
                    .Where(x => x.OrderLineId == _orderLineId)
                    .Select(x => x.Truck)
                    .Distinct()
                    .Select(x => new
                    {
                        TruckId = x.Id,
                        x.VehicleCategory.AssetType
                    })
                    .ToListAsync();

                var nextTimeOnJobUtc = orderLine.FirstStaggeredTimeOnJob;
                foreach (var orderLineTruck in orderLineTrucks)
                {
                    if (orderLineTruck.TimeOnJob != nextTimeOnJobUtc)
                    {
                        orderLineTruck.TimeOnJob = nextTimeOnJobUtc;
                        affectedTruckIds.Add(orderLineTruck.TruckId);
                    }

                    var truck = truckData.FirstOrDefault(x => x.TruckId == orderLineTruck.TruckId);
                    if (truck == null || truck.AssetType == AssetType.Trailer)
                    {
                        continue;
                    }

                    nextTimeOnJobUtc = nextTimeOnJobUtc?.AddMinutes(orderLine.StaggeredTimeInterval ?? 0);
                }
            }
        }

        private async Task SyncLinkedOrderLines()
        {
            if (_orderLineId == 0)
            {
                return;
            }
            await _crossTenantOrderSender.SyncLinkedOrderLines(_orderLineId, _alreadySyncedOrderLineIds, _updatedFields, _orderLineUpdaterFactory);
        }

        private IReadonlyChecker<OrderLine> _readonlyChecker = null;
        public IReadonlyChecker<OrderLine> GetReadonlyChecker(OrderLine entity)
        {
            return _readonlyChecker ?? (_readonlyChecker = _orderLineReadonlyCheckerFactory.Create(_orderLineId).SetEntity(entity));
        }

        public async Task UpdateOrderModificationTime()
        {
            var order = await GetOrderAsync();
            order.LastModificationTime = Clock.Now;
            order.LastModifierUserId = _abpSession.UserId;
        }

        public async Task SaveChangesAsync()
        {
            var orderLine = await GetEntityAsync();

            orderLine.RemoveStaggeredTimeIfNeeded();

            if (!orderLine.IsQuantityValid())
            {
                throw new UserFriendlyException(L("QuantityIsRequiredWhenTotalIsSpecified"));
            }

            await ValidateOverriddenTotalsIfNeeded(orderLine);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_orderLineId != 0 && (orderLine.MaterialQuantity ?? 0) == 0 && (orderLine.FreightQuantity ?? 0) == 0 && (orderLine.NumberOfTrucks ?? 0) == 0)
            {
                await _orderLineScheduledTrucksUpdater.DeleteOrderLineTrucks(new DeleteOrderLineTrucksInput
                {
                    OrderLineId = _orderLineId
                });
                await _unitOfWorkManager.Current.SaveChangesAsync();
            }

            if (_orderLineId == 0)
            {
                await _orderLineRepository.InsertAsync(orderLine);
            }

            await UpdateOrderModificationTime();

            if (_needToUpdateStaggeredTimeOnTrucks)
            {
                await UpdateStaggeredTimeOnTrucks();
                _needToUpdateStaggeredTimeOnTrucks = false;
            }

            if (_affectedDispatches.Any())
            {
                _affectedDispatches.ForEach(d => d.LastModificationTime = Clock.Now);
                await _unitOfWorkManager.Current.SaveChangesAsync();
                foreach (var dispatchGroup in _affectedDispatches.GroupBy(x => x.DriverId))
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                    {
                        LogMessage = $"Updated OrderLine has affected dispatch(es) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, _affectedDispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage("Updated OrderLine has affected dispatch(es)"));
                _affectedDispatches = new List<Dispatch>();
            }

            if (_needToRecalculateFuelSurcharge)
            {
                await _unitOfWorkManager.Current.SaveChangesAsync();
                await _fuelSurchargeCalculator.RecalculateOrderLinesWithTickets(orderLine.Id);
                _needToRecalculateFuelSurcharge = false;
            }

            if (_updatedFields.Any())
            {
                await _unitOfWorkManager.Current.SaveChangesAsync();
                await SyncLinkedOrderLines();
                _updatedFields.Clear();
            }
        }
    }
}
