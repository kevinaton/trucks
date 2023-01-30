using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class GetTenantStatisticsResult : PagedResultDto<TenantStatisticsDto>
    {
        public TenantStatisticsDto Total { get; set; }

        public GetTenantStatisticsResult()
        {
        }

        public GetTenantStatisticsResult(int totalCount, IReadOnlyList<TenantStatisticsDto> items) : base(totalCount, items)
        {
        }
    }
}
