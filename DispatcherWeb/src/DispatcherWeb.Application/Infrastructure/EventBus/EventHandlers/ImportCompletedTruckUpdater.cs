using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.Identity;
using DispatcherWeb.Imports.Services;
using DispatcherWeb.Infrastructure.EventBus.Events;

namespace DispatcherWeb.Infrastructure.EventBus.EventHandlers
{
    public class ImportCompletedTruckUpdater : IAsyncEventHandler<ImportCompletedEventData>, ITransientDependency
    {
        private readonly IUpdateTruckFromImportAppService _updateTruckFromImportAppService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ImportCompletedTruckUpdater(
            IUpdateTruckFromImportAppService updateTruckFromImportAppService,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _updateTruckFromImportAppService = updateTruckFromImportAppService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(ImportCompletedEventData eventData)
        {
            if (eventData.Args.ImportType == ImportType.VehicleUsage)
            {
                using (var uow = _unitOfWorkManager.Begin())
                {
                    _updateTruckFromImportAppService.UpdateMileageAndHours(eventData.Args.RequestorUser.GetTenantId(), eventData.Args.RequestorUser.UserId);
                    await uow.CompleteAsync();
                }
            }
        }
    }
}
