using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Threading;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.BackgroundJobs
{
    public class RecalculateHasAllActualAmountsValuesBackgroundJob : AsyncBackgroundJob<RecalculateHasAllActualAmountsValuesBackgroundJobArgs>, ITransientDependency
    {
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IAppNotifier _appNotifier;

        public RecalculateHasAllActualAmountsValuesBackgroundJob(
            OrderTaxCalculator orderTaxCalculator,
            IAppNotifier appNotifier
            )
        {
            _orderTaxCalculator = orderTaxCalculator;
            _appNotifier = appNotifier;
        }

        public async override Task ExecuteAsync(RecalculateHasAllActualAmountsValuesBackgroundJobArgs args)
        {
            try
            {
                var result = AsyncHelper.RunSync(_orderTaxCalculator.UpdateHasAllActualAmountsValues);
                await _appNotifier.SendMessageAsync(args.RequestorUser, $"Recalculation of HasAllActualAmounts values finished successfully, {result.ProcessedOrdersCount} orders processed", NotificationSeverity.Success);
                //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, $"Recalculation of HasAllActualAmounts values finished successfully, {result.ProcessedOrdersCount} orders processed", NotificationSeverity.Success));
            }
            catch (Exception)
            {
                await _appNotifier.SendMessageAsync(args.RequestorUser, "Recalculation of HasAllActualAmounts values failed", NotificationSeverity.Error);
                //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, "Recalculation of HasAllActualAmounts values failed", NotificationSeverity.Error));
                throw;
            }

        }
    }
}
