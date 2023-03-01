using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class GetRequestsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "LastMonthNumberOfTransactions desc";
            }
        }
    }
}
