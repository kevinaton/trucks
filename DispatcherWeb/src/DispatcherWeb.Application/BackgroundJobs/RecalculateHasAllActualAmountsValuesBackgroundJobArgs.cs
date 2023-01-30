using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class RecalculateHasAllActualAmountsValuesBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
    }
}
