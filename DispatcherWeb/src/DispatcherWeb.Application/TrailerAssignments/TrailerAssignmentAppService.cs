using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TrailerAssignments.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TrailerAssignments
{
    [AbpAuthorize]
    public class TrailerAssignmentAppService : DispatcherWebAppServiceBase, ITrailerAssignmentAppService
    {
        private readonly IRepository<TrailerAssignment> _trailerAssignmentRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly ISyncRequestSender _syncRequestSender;

        public TrailerAssignmentAppService(
            IRepository<TrailerAssignment> trailerAssignmentRepository,
            IRepository<Truck> truckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IDriverApplicationPushSender driverApplicationPushSender,
            ISyncRequestSender syncRequestSender
            )
        {
            _trailerAssignmentRepository = trailerAssignmentRepository;
            _truckRepository = truckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _driverApplicationPushSender = driverApplicationPushSender;
            _syncRequestSender = syncRequestSender;
        }

        public async Task SetTrailerForTractor(SetTrailerForTractorInput input)
        {
            var trailerAssignment = await GetTrailerAssignmentQuery(input)
                .Where(x => input.TractorId == x.TractorId)
                .FirstOrDefaultAsync() ?? new TrailerAssignment
                {
                    Date = input.Date,
                    Shift = input.Shift,
                    OfficeId = input.OfficeId,
                    TractorId = input.TractorId
                };

            if (trailerAssignment.Id == 0)
            {
                _trailerAssignmentRepository.Insert(trailerAssignment);
            }

            trailerAssignment.TrailerId = input.TrailerId;
            await CurrentUnitOfWork.SaveChangesAsync();

            await RemoveTrailerAssignmentDuplicates(new RemoveTrailerAssignmentDuplicatesInput
            {
                Date = input.Date,
                Shift = input.Shift,
                OfficeId = input.OfficeId,
                TractorId = input.TractorId,
                TrailerId = input.TrailerId,
            });
        }

        public async Task SetTractorForTrailer(SetTractorForTrailerInput input)
        {
            if (input.TractorId.HasValue)
            {
                await SetTrailerForTractor(new SetTrailerForTractorInput()
                {
                    Date = input.Date,
                    Shift = input.Shift,
                    OfficeId = input.OfficeId,
                    TractorId = input.TractorId.Value,
                    TrailerId = input.TrailerId,
                });
            }
            else
            {
                await RemoveTrailerAssignmentDuplicates(new RemoveTrailerAssignmentDuplicatesInput()
                {
                    Date = input.Date,
                    Shift = input.Shift,
                    OfficeId = input.OfficeId,
                    TractorId = null,
                    TrailerId = input.TrailerId,
                });
            }
        }

        private IQueryable<TrailerAssignment> GetTrailerAssignmentQuery(TrailerAssignmentInputBase input)
        {
            return _trailerAssignmentRepository.GetAll()
                .Where(x => input.Date == x.Date
                    && input.Shift == x.Shift
                    && input.OfficeId == x.OfficeId
                );
        }

        private async Task RemoveTrailerAssignmentDuplicates(RemoveTrailerAssignmentDuplicatesInput input)
        {
            if (input.TrailerId.HasValue)
            {
                //make sure there are no other tractors with the target trailer assigned

                var incorrectTrailerAssignments = await GetTrailerAssignmentQuery(input)
                    .Where(x => x.TrailerId == input.TrailerId && x.TractorId != input.TractorId)
                    .ToListAsync();
                if (incorrectTrailerAssignments.Any())
                {
                    incorrectTrailerAssignments.ForEach(x => x.TrailerId = null);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                var tractorIdsWithDefaultTargetTrailer = await _truckRepository.GetAll()
                    .Where(x => x.DefaultTrailerId == input.TrailerId && x.Id != input.TractorId)
                    .Select(x => x.Id)
                    .ToListAsync();
                if (tractorIdsWithDefaultTargetTrailer.Any())
                {
                    var trailerAssignments = await GetTrailerAssignmentQuery(input)
                        .Where(x => tractorIdsWithDefaultTargetTrailer.Contains(x.TractorId))
                        .ToListAsync();

                    foreach (var tractorId in tractorIdsWithDefaultTargetTrailer)
                    {
                        if (!trailerAssignments.Any(x => x.TractorId == tractorId))
                        {
                            _trailerAssignmentRepository.Insert(new TrailerAssignment
                            {
                                Date = input.Date,
                                Shift = input.Shift,
                                OfficeId = input.OfficeId,
                                TractorId = tractorId,
                                TrailerId = null,
                            });
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
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
                .Where(x => x.OrderLineTruckId == orderLineTruck.OrderLineId)
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
    }
}
