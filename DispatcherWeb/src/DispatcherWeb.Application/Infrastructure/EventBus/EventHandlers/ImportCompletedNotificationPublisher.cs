using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using Abp.Notifications;
using DispatcherWeb.Infrastructure.EventBus.Events;
using DispatcherWeb.Infrastructure.General;
using DispatcherWeb.Infrastructure.Notifications;
using DispatcherWeb.Notifications;
using DispatcherWeb.Url;

namespace DispatcherWeb.Infrastructure.EventBus.EventHandlers
{
    public class ImportCompletedNotificationPublisher : IAsyncEventHandler<ImportCompletedEventData>, ITransientDependency
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IWebUrlService _webUrlService;
        private readonly INotAuthorizedUserAppService _notAuthorizedUserService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ImportCompletedNotificationPublisher(
            INotificationPublisher notificationPublisher,
            IWebUrlService webUrlService,
            INotAuthorizedUserAppService notAuthorizedUserService,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _notificationPublisher = notificationPublisher;
            _webUrlService = webUrlService;
            _notAuthorizedUserService = notAuthorizedUserService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [UnitOfWork]
        public async Task HandleEventAsync(ImportCompletedEventData eventData)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await PublishNotificationAsync(eventData.Args.RequestorUser, eventData.Args.File);

                await uow.CompleteAsync();
            }
        }
        private async Task PublishNotificationAsync(UserIdentifier user, string file)
        {
            var tenancyName = _notAuthorizedUserService.GetTenancyNameOrNull(user.TenantId);
            string fileLink = $"{_webUrlService.GetSiteRootAddress(tenancyName)}app/ImportResults/{file}";
            await _notificationPublisher.PublishAsync(AppNotificationNames.ImportCompleted,
                new ImportCompletedNotificationData(fileLink),
                userIds: new[] { user },
                severity: NotificationSeverity.Success);
        }

    }
}
