using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Timing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.WebPush;

namespace DispatcherWeb.SyncRequests
{
    public class FirebasePushSenderBackgroundJob : AsyncBackgroundJob<FirebasePushSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IFirebasePushSender _firebasePushSender;
        private readonly IRepository<FcmPushMessage, Guid> _fcmPushMessageRepository;

        public FirebasePushSenderBackgroundJob(
            IFirebasePushSender firebasePushSender,
            IRepository<FcmPushMessage, Guid> fcmPushMessageRepository
            )
        {
            _firebasePushSender = firebasePushSender;
            _fcmPushMessageRepository = fcmPushMessageRepository;
        }

        [UnitOfWork(isTransactional: false)]
        public override async Task ExecuteAsync(FirebasePushSenderBackgroundJobArgs args)
        {
            using (UnitOfWorkManager.Current.SetTenantId(args.RequestorUser.TenantId))
            {
                var pushMessage = await _fcmPushMessageRepository.FirstOrDefaultAsync(x => x.Id == args.PushMessageGuid);
                try
                {
                    await _firebasePushSender.SendAsync(args.RegistrationToken, args.JsonPayload);
                    if (pushMessage != null)
                    {
                        pushMessage.SentAtDateTime = Clock.Now;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                    if (pushMessage != null)
                    {
                        pushMessage.Error = (ex.Message ?? ex.ToString())?.Truncate(EntityStringFieldLengths.FcmPushMessage.Error);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    //await _appNotifier.SendMessageAsync(args.RequestorUser, "Sending sync request failed", NotificationSeverity.Error);
                    if (ex is not FirebaseAdmin.Messaging.FirebaseMessagingException)
                    {
                        throw;
                    }
                    else
                    {
                        //todo maybe we have to remove the subscription for some specific ex.Message values
                    }
                }
            }
        }
    }
}
