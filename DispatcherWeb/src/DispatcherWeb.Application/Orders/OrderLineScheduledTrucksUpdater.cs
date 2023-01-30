using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders
{
    [RemoteService(false)]
    public class OrderLineScheduledTrucksUpdater : DispatcherWebAppServiceBase, IOrderLineScheduledTrucksUpdater
    {
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IDispatchingAppService _dispatchingAppService;

        public OrderLineScheduledTrucksUpdater(
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<OrderLine> orderLineRepository,
            IDispatchingAppService dispatchingAppService
        )
        {
            _orderLineTruckRepository = orderLineTruckRepository;
            _orderLineRepository = orderLineRepository;
            _dispatchingAppService = dispatchingAppService;
        }

        public async Task UpdateScheduledTrucks(OrderLine orderLine, double? scheduledTrucks)
        {
            scheduledTrucks = scheduledTrucks.Round(2);

            await EnsureNumberOfTrucksIsNotLessCurrentUtilization();
            
            orderLine.ScheduledTrucks = scheduledTrucks;

            // Local functions
            async Task EnsureNumberOfTrucksIsNotLessCurrentUtilization()
            {
                var newNumberOfTrucks = Convert.ToDecimal(scheduledTrucks ?? 0);
                if (newNumberOfTrucks < Convert.ToDecimal(orderLine.NumberOfTrucks) && newNumberOfTrucks < await GetOrderLineUtilization(orderLine.Id))
                {
                    throw new ApplicationException("Cannot set number of trucks less than the current utilization!");
                }
            }
        }

        public async Task<decimal> GetOrderLineUtilization(int orderLineId) =>
            await _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLineId == orderLineId && olt.Truck.VehicleCategory.IsPowered)
                .SumAsync(olt => olt.Utilization);

        public async Task DeleteOrderLineTrucks(DeleteOrderLineTrucksInput input)
        {
            if (input.OrderLineId.HasValue)
            {
                var hasAcknowledgedOrLoadedDispatches = await _orderLineRepository.GetAll()
                    .AnyAsync(ol =>
                        ol.Id == input.OrderLineId.Value &&
                        ol.Dispatches.Any(d => Dispatch.AcknowledgedOrLoadedStatuses.Contains(d.Status))
                    );
                if (hasAcknowledgedOrLoadedDispatches)
                {
                    throw new UserFriendlyException("There is an acknowledged or loaded dispatch!");
                }

                await _dispatchingAppService.CancelDispatches(new CancelDispatchesInput
                {
                    OrderLineId = input.OrderLineId.Value,
                    CancelDispatchStatuses = Dispatch.OutstandingDispatchStatuses
                });

                if (input.MarkAsDone)
                {
                    var orderLineTrucks = await _orderLineTruckRepository.GetAll().Where(x => x.OrderLineId == input.OrderLineId.Value).ToListAsync();
                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.IsDone = true;
                        orderLineTruck.Utilization = 0;
                    }
                }
                else
                {
                    await _orderLineTruckRepository.DeleteAsync(x => x.OrderLineId == input.OrderLineId.Value);
                }

            }
            else if (input.TruckId.HasValue && input.Date.HasValue)
            {
                var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                    .Where(x => x.TruckId == input.TruckId && x.OrderLine.Order.DeliveryDate == input.Date && x.OrderLine.Order.Shift == input.Shift)
                    .Select(x => new
                    {
                        x.Id,
                        x.OrderLineId
                    })
                    .ToListAsync();

                if (!orderLineTrucks.Any())
                {
                    return;
                }

                var orderLineIds = orderLineTrucks.Select(x => x.OrderLineId).ToList();

                var hasAcknowledgedOrLoadedDispatches = await _orderLineRepository.GetAll()
                    .AnyAsync(ol =>
                        orderLineIds.Contains(ol.Id) &&
                        ol.Dispatches.Any(d => Dispatch.AcknowledgedOrLoadedStatuses.Contains(d.Status) && d.TruckId == input.TruckId)
                    );
                if (hasAcknowledgedOrLoadedDispatches)
                {
                    throw new UserFriendlyException("There is an acknowledged or loaded dispatch!");
                }

                await _dispatchingAppService.CancelDispatches(new CancelDispatchesInput
                {
                    TruckId = input.TruckId,
                    Date = input.Date,
                    Shift = input.Shift,
                    CancelDispatchStatuses = Dispatch.OutstandingDispatchStatuses
                });

                var orderlineTruckIds = orderLineTrucks.Select(x => x.Id).ToList();
                await _orderLineTruckRepository.DeleteAsync(x => orderlineTruckIds.Contains(x.Id));
            }
            else
            {
                throw new ApplicationException("Either OrderLineId or (TruckId, Date) is required");
            }
        }


    }
}
