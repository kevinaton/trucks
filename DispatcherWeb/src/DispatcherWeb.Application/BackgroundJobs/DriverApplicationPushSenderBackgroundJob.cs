using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Threading;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverApplicationPushSenderBackgroundJob : AsyncBackgroundJob<DriverApplicationPushSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly IAppNotifier _appNotifier;

        public DriverApplicationPushSenderBackgroundJob(
            IDriverApplicationPushSender driverApplicationPushSender,
            IAppNotifier appNotifier
            )
        {
            _driverApplicationPushSender = driverApplicationPushSender;
            _appNotifier = appNotifier;
        }

        [UnitOfWork]
        public async override Task ExecuteAsync(DriverApplicationPushSenderBackgroundJobArgs args)
        {
            try
            {
                using (UnitOfWorkManager.Current.SetTenantId(args.TenantId))
                {
                    await _driverApplicationPushSender.SendPushMessageToDriversImmediately(args);
                    //AsyncHelper.RunSync(async () => await _driverApplicationPushSender.SendPushMessageToDriversImmediately(args));
                }
            }
            catch (Exception)
            {
                await _appNotifier.SendMessageAsync(args.RequestorUser, "Sending sync request failed", NotificationSeverity.Error);
                //AsyncHelper.RunSync(async () => await  _appNotifier.SendMessageAsync(args.RequestorUser, "Sending sync request failed", NotificationSeverity.Error));
                throw;
            }
            //AsyncHelper.RunSync(() => _appNotifier.SendMessageAsync(args.RequestorUser, "Sent successfully", NotificationSeverity.Success));
        }
    }
}
