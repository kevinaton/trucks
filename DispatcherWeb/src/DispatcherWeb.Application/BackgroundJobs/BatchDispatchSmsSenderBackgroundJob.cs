using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Notifications;

namespace DispatcherWeb.BackgroundJobs
{
    [Obsolete]
    public class BatchDispatchSmsSenderBackgroundJob : AsyncBackgroundJob<BatchDispatchSmsSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IDispatchSender _dispatchSender;
        private readonly IAppNotifier _appNotifier;
        private readonly IAbpSession _abpSession;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public BatchDispatchSmsSenderBackgroundJob(
            IDispatchSender dispatchSender,
            IAppNotifier appNotifier,
            IAbpSession abpSession,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _dispatchSender = dispatchSender;
            _appNotifier = appNotifier;
            _abpSession = abpSession;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override async Task ExecuteAsync(BatchDispatchSmsSenderBackgroundJobArgs args)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (CurrentUnitOfWork.SetTenantId(args.RequestorUser.TenantId))
            using (_abpSession.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            {
                try
                {
                    await _dispatchSender.BatchSendSms(args.SendSmsInputs);
                    uow.Complete();
                }
                catch (UserFriendlyException ex)
                {
                    await _appNotifier.SendMessageAsync(args.RequestorUser, ex.Message, NotificationSeverity.Error);
                    return;
                }
            }
        }
    }
}
