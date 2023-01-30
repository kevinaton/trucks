﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public class HostDashboardAppService_GetDashboardStatisticsData_Day_Fixture : HostDashboardAppService_GetDashboardStatisticsData_Fixture
    {
        protected override (DateTime startDate, DateTime endDate) GetInterval()
        {
            DateTime date = DateTime.UtcNow.Date.AddDays(-1);
            return (date, date.AddDays(1).AddMilliseconds(-1));
        }

    }
}
