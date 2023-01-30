using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Loads.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Loads
{
    [AbpAuthorize]
    public class LoadAppService : DispatcherWebDriverAppAppServiceBase, ILoadAppService
    {
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Load> _loadRepository;

        public LoadAppService(
            IRepository<Dispatch> dispatchRepository,
            IRepository<Load> loadRepository
            )
        {
            _dispatchRepository = dispatchRepository;
            _loadRepository = loadRepository;
        }

        public async Task<LoadDto> Post(LoadDto model)
        {
            var load = model.Id == 0 ? new Load() : await _loadRepository.GetAsync(model.Id);

            if (load == null)
            {
                throw new UserFriendlyException($"Load with id ${model.Id} wasn't found");
            }

            if (!await _dispatchRepository.GetAll().AnyAsync(x => x.Id == model.DispatchId))
            {
                throw new UserFriendlyException($"Dispatch with id {model.DispatchId} wasn't found");
            }

            if (!await _dispatchRepository.GetAll().AnyAsync(x => x.Id == model.DispatchId && x.Driver.UserId == Session.UserId))
            {
                throw new UserFriendlyException($"You cannot edit dispatches assigned to other users");
            }

            load.SourceDateTime = model.SourceDateTime;
            load.SourceLatitude = model.SourceLatitude;
            load.SourceLongitude = model.SourceLongitude;
            load.DestinationDateTime = model.DestinationDateTime;
            load.DestinationLatitude = model.DestinationLatitude;
            load.DestinationLongitude = model.DestinationLongitude;
            load.SignatureId = model.SignatureId;
            load.SignatureName = model.SignatureName;

            if (load.Id == 0)
            {
                load.DispatchId = model.DispatchId;

                await _loadRepository.InsertAndGetIdAsync(load);
                model.Id = load.Id;
            }

            return model;
        }
    }
}
