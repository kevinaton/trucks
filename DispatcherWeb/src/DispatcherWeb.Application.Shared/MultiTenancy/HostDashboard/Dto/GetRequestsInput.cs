using Abp.Runtime.Validation;
using DispatcherWeb.Dto;
using System;
using System.Collections.Generic;
using System.Text;

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
