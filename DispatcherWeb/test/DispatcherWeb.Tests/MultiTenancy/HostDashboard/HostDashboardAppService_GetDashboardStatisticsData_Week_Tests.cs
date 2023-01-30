using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public class HostDashboardAppService_GetDashboardStatisticsData_Week_Tests : HostDashboardAppService_GetDashboardStatisticsData_Tests, IClassFixture<HostDashboardAppService_GetDashboardStatisticsData_Week_Fixture>
    {
        public HostDashboardAppService_GetDashboardStatisticsData_Week_Tests(HostDashboardAppService_GetDashboardStatisticsData_Week_Fixture fixture)
        {
            _fixture = fixture;
        }


    }
}
