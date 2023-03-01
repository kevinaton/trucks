using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using Abp.Notifications;
using DispatcherWeb.Infrastructure.EventBus.Events;
using DispatcherWeb.Notifications;

namespace DispatcherWeb.Infrastructure.EventBus.EventHandlers
{
    public class ImportFailedNotificationPublisher : IAsyncEventHandler<ImportFailedEventData>, ITransientDependency
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ImportFailedNotificationPublisher(
            INotificationPublisher notificationPublisher,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _notificationPublisher = notificationPublisher;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(ImportFailedEventData eventData)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await PublishNotificationAsync(eventData.Args.RequestorUser);
            }
        }
        private async Task PublishNotificationAsync(UserIdentifier user)
        {
            await _notificationPublisher.PublishAsync(AppNotificationNames.SimpleMessage,
                new MessageNotificationData("Import failed because of unknown error."),
                userIds: new[] { user },
                severity: NotificationSeverity.Error);
        }

    }
}
