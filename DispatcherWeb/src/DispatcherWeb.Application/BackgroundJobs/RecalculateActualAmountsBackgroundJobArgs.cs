using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class RecalculateActualAmountsBackgroundJobArgs
    {
        public int TenantId { get; set; }
        public UserIdentifier RequestorUser { get; set; }
    }
}
