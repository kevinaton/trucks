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
        private readonly IRepository<FcmRegistrationToken> _fcmRegistrationTokenRepository;

        public FirebasePushSenderBackgroundJob(
            IFirebasePushSender firebasePushSender,
            IRepository<FcmPushMessage, Guid> fcmPushMessageRepository,
            IRepository<FcmRegistrationToken> fcmRegistrationTokenRepository
            )
        {
            _firebasePushSender = firebasePushSender;
            _fcmPushMessageRepository = fcmPushMessageRepository;
            _fcmRegistrationTokenRepository = fcmRegistrationTokenRepository;
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
                        if (ex.Message?.IsIn("Requested entity was not found.", "The registration token is not a valid FCM registration token") == true)
                        {
                            Logger.Info($"Removing registration token {args.RegistrationToken.Id} because it was rejected by FCM");
                            await _fcmRegistrationTokenRepository.DeleteAsync(x => x.Id == args.RegistrationToken.Id);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}
