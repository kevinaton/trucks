using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.TrailerAssignments.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TrailerAssignments
{
    [AbpAuthorize]
    public class TrailerAssignmentAppService : DispatcherWebAppServiceBase, ITrailerAssignmentAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly ISyncRequestSender _syncRequestSender;

        public TrailerAssignmentAppService(
            IRepository<Truck> truckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IDriverApplicationPushSender driverApplicationPushSender,
            ISyncRequestSender syncRequestSender
            )
        {
            _truckRepository = truckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _driverApplicationPushSender = driverApplicationPushSender;
            _syncRequestSender = syncRequestSender;
        }

        public async Task SetTrailerForTractor(SetTrailerForTractorInput input)
        {
            var syncRequest = new SyncRequest();

            var tractor = await _truckRepository.GetAll()
                .Where(x => x.Id == input.TractorId)
                .FirstAsync();

            var oldTrailerId = tractor.CurrentTrailerId;
            if (oldTrailerId.HasValue)
            {
                syncRequest.AddChange(EntityEnum.Truck, GetChangedTruckById(oldTrailerId.Value));
            }

            tractor.CurrentTrailerId = input.TrailerId;
            syncRequest.AddChange(EntityEnum.Truck, tractor.ToChangedEntity());

            if (input.TrailerId.HasValue)
            {
                var otherTractors = await _truckRepository.GetAll()
                    .Where(x => x.CurrentTrailerId == input.TrailerId.Value && x.Id != input.TractorId)
                    .ToListAsync();

                otherTractors.ForEach(x => {
                    x.CurrentTrailerId = null;
                    syncRequest.AddChange(EntityEnum.Truck, x.ToChangedEntity());
                });

                syncRequest.AddChange(EntityEnum.Truck, GetChangedTruckById(input.TrailerId.Value));
            }

            if (input.UpdateExistingOrderLineTrucks)
            {
                var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                    .Where(x => input.Date == x.OrderLine.Order.DeliveryDate && !x.IsDone)
                    .Where(x => x.TrailerId == oldTrailerId)
                    .Where(x => input.TractorId == x.TruckId)
                    .ToListAsync();

                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.TrailerId = input.TrailerId;
                    syncRequest.AddChange(EntityEnum.OrderLine, GetChangedOrderLineById(orderLineTruck.OrderLineId));
                }

                var orderLineTruckIds = orderLineTrucks.Select(x => x.Id).ToList();

                var affectedDispatches = await _dispatchRepository.GetAll()
                    .Where(x => x.OrderLineTruckId.HasValue && orderLineTruckIds.Contains(x.OrderLineTruckId.Value))
                    .ToListAsync();

                await SendSyncRequestForAffectedDispatches(affectedDispatches, "Updated Trailer for dispatch(es)");
            }

            // send sync requests for affected tractor, trailers and orderlines
            await _syncRequestSender.SendSyncRequest(syncRequest);
        }

        public async Task SetTractorForTrailer(SetTractorForTrailerInput input)
        {
            var syncRequest = new SyncRequest();

            var otherTractors = await _truckRepository.GetAll()
                .Where(x => x.CurrentTrailerId == input.TrailerId && x.Id != input.TractorId)
                .ToListAsync();

            otherTractors.ForEach(x => {
                x.CurrentTrailerId = null;
                syncRequest.AddChange(EntityEnum.Truck, x.ToChangedEntity());
            });

            if (input.TractorId.HasValue)
            {
                var tractor = await _truckRepository.GetAll()
                    .Where(x => x.Id == input.TractorId.Value)
                    .FirstAsync();

                tractor.CurrentTrailerId = input.TrailerId;
                syncRequest.AddChange(EntityEnum.Truck, tractor.ToChangedEntity());
            }

            syncRequest.AddChange(EntityEnum.Truck, GetChangedTruckById(input.TrailerId));

            // send sync requests for affected tractor and trailer
            await _syncRequestSender.SendSyncRequest(syncRequest);
        }

        public async Task SetTrailerForOrderLineTruck(SetTrailerForOrderLineTruckInput input)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAll().FirstAsync(x => x.Id == input.OrderLineTruckId);
            if (orderLineTruck.TrailerId == input.TrailerId)
            {
                return;
            }

            orderLineTruck.TrailerId = input.TrailerId;

            var affectedDispatches = await _dispatchRepository.GetAll()
                .Where(x => x.OrderLineTruckId == input.OrderLineTruckId)
                .ToListAsync();

            await SendSyncRequestForAffectedDispatches(affectedDispatches, "Updated Trailer for dispatch(es)");
        }

        private async Task SendSyncRequestForAffectedDispatches(List<Dispatch> affectedDispatches, string logMessage)
        {
            if (affectedDispatches.Any())
            {
                affectedDispatches.ForEach(d => d.LastModificationTime = Clock.Now);
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var dispatchGroup in affectedDispatches.GroupBy(x => x.DriverId))
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                    {
                        LogMessage = $"{logMessage} {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, affectedDispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage(logMessage));
            }
        }

        private ChangedTruck GetChangedTruckById(int id)
        {
            return new ChangedTruck
            {
                Id = id
            };
        }

        private ChangedOrderLine GetChangedOrderLineById(int id)
        {
            return new ChangedOrderLine
            {
                Id = id
            };
        }
    }
}
