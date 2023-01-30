using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Threading;
using DispatcherWeb.Notifications;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.BackgroundJobs
{
    public class RecalculateActualAmountsBackgroundJob : AsyncBackgroundJob<RecalculateActualAmountsBackgroundJobArgs>, ITransientDependency
    {
        private readonly ITicketAppService _ticketAppService;
        private readonly IAppNotifier _appNotifier;

        public RecalculateActualAmountsBackgroundJob(
            ITicketAppService ticketAppService,
            IAppNotifier appNotifier
            )
        {
            _ticketAppService = ticketAppService;
            _appNotifier = appNotifier;
        }

        [UnitOfWork]
        public async override Task ExecuteAsync(RecalculateActualAmountsBackgroundJobArgs args)
        {
            try
            {
                using (UnitOfWorkManager.Current.SetTenantId(args.TenantId))
                {
                    AsyncHelper.RunSync(_ticketAppService.RecalculateAllOfficeAmountsAsync);
                }
            }
            catch (Exception)
            {
                await _appNotifier.SendMessageAsync(args.RequestorUser, "Actual Quantity recalculation failed", NotificationSeverity.Error);
                //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, "Actual Quantity recalculation failed", NotificationSeverity.Error));
                throw;
            }
            await _appNotifier.SendMessageAsync(args.RequestorUser, "Actual Quantity recalculation finished successfully", NotificationSeverity.Success);
            //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, "Actual Quantity recalculation finished successfully", NotificationSeverity.Success));
        }
    }
}
