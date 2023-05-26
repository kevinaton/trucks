using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
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

        public TrailerAssignmentAppService(
            IRepository<TrailerAssignment> trailerAssignmentRepository,
            IRepository<Truck> truckRepository
            )
        {
            _trailerAssignmentRepository = trailerAssignmentRepository;
            _truckRepository = truckRepository;
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

            if (input.TractorId.HasValue)
            {
                
            }
        }
    }
}
