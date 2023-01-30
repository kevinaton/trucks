using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Notifications;
using Abp.Threading;
using Abp.UI;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Receipts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.BackgroundJobs
{
    public class CopyReceiptsFromActualAmountsBackgroundJob : AsyncBackgroundJob<CopyReceiptsFromActualAmountsBackgroundJobArgs>, ITransientDependency
    {
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IAppNotifier _appNotifier;
        private readonly ReceiptSeeder _receiptSeeder;

        public CopyReceiptsFromActualAmountsBackgroundJob(
            OrderTaxCalculator orderTaxCalculator,
            IAppNotifier appNotifier,
            ReceiptSeeder receiptSeeder
            )
        {
            _orderTaxCalculator = orderTaxCalculator;
            _appNotifier = appNotifier;
            _receiptSeeder = receiptSeeder;
        }

        public async override Task ExecuteAsync(CopyReceiptsFromActualAmountsBackgroundJobArgs args)
        {
            try
            {
                await _receiptSeeder.SeedReceiptsFromActualAmounts(args);
                await _appNotifier.SendMessageAsync(args.RequestorUser, $"Copying of ActualAmounts to Receipts finished successfully", NotificationSeverity.Success);
                //AsyncHelper.RunSync(async () => await _receiptSeeder.SeedReceiptsFromActualAmounts(args));
                //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, $"Copying of ActualAmounts to Receipts finished successfully", NotificationSeverity.Success));
            }
            catch (UserFriendlyException ex)
            {
                await _appNotifier.SendMessageAsync(args.RequestorUser, ex.Message, NotificationSeverity.Error);
                //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, ex.Message, NotificationSeverity.Error));
                return;
            }
            catch (Exception)
            {
                await _appNotifier.SendMessageAsync(args.RequestorUser, "Copying of ActualAmounts to Receipts failed", NotificationSeverity.Error);
                //AsyncHelper.RunSync(async () => await _appNotifier.SendMessageAsync(args.RequestorUser, "Copying of ActualAmounts to Receipts failed", NotificationSeverity.Error));
                throw;
            }

        }
    }
}
