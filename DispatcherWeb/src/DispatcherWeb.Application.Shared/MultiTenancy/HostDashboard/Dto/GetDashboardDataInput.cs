using Abp.Runtime.Validation;
using DispatcherWeb.Dto;
using System;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class GetDashboardDataInput : PagedAndSortedInputDto, IShouldNormalize//: DashboardInputBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ChartDateInterval IncomeStatisticsDateInterval { get; set; }
        public int? EditionId { get; set; }
        public FilterActiveStatus? Status { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "TenantName";
            	TrimTime();
        	}


  	}
      private void TrimTime()
        {
            StartDate = StartDate.Date;
            StartDate = StartDate.Date;

        }
    }
}
