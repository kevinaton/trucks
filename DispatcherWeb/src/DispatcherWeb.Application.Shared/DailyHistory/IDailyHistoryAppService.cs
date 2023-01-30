using System;

namespace DispatcherWeb.DailyHistory
{
    public interface IDailyHistoryAppService
    {
        void FillDailyHistories();
        void FillTenantDailyHistory(DateTime todayUtc);
        void FillUserDailyHistory(DateTime todayUtc);
        void FillTransactionDailyHistory(DateTime todayUtc);
    }
}