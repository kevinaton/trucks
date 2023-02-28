using System;
using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Notifications;
using Abp.Threading;
using DispatcherWeb.Notifications;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Infrastructure.BackgroundJobs
{
    public class UpdateMileageJob : AsyncBackgroundJob<UpdateMileageJobArgs>, ITransientDependency
    {
        private readonly ITruckTelematicsAppService _truckTelematicsAppService;
        private readonly INotificationPublisher _notificationPublisher;

        public UpdateMileageJob(
            ITruckTelematicsAppService truckTelematicsAppService,
            INotificationPublisher notificationPublisher
        )
        {
            _truckTelematicsAppService = truckTelematicsAppService;
            _notificationPublisher = notificationPublisher;
        }

        public async override Task ExecuteAsync(UpdateMileageJobArgs args)
        {
            try
            {
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.MileageUpdateCompleted,
                    new MessageNotificationData("We are updating your truck mileages. This is a slow process, so we will notify you when it is done."),
                    null,
                    NotificationSeverity.Info,
                    userIds: new[] { new UserIdentifier(args.TenantId, args.UserId) }
                );
                var result = AsyncHelper.RunSync(() => _truckTelematicsAppService.UpdateMileageForTenantAsync(args.TenantId, args.UserId));
                string ignoredTrucksMessage = result.trucksIgnored != 0 ? $"Ignored (don't exist in the DB) {result.trucksIgnored} trucks." : "";
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.MileageUpdateCompleted,
                    new MessageNotificationData($"Updating mileage has finished successfully. Updated {result.trucksUpdated} trucks. {ignoredTrucksMessage}"),
                    null,
                    NotificationSeverity.Success,
                    userIds: new[] { new UserIdentifier(args.TenantId, args.UserId) }
                );
            }
            catch (Exception e)
            {
                Logger.Error($"Error when updating mileage: {e}");
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.MileageUpdateError,
                    new MessageNotificationData("Updating mileage failed."),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[] { new UserIdentifier(args.TenantId, args.UserId) }
                );
            }
        }
    }
}
