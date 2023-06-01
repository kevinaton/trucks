using System;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTenantStatisticsInput
    {
        public int? TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
