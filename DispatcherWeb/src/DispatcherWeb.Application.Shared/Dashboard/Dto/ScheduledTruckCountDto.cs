using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dashboard.Dto
{
    public class ScheduledTruckCountDto
    {
        public double? TrucksRequestedTodayCount { get; set; }
        public double? TrucksScheduledForTodayCount { get; set; }
        public double? TrucksScheduledForTomorrowCount { get; set; }
        public double? TrucksRequestedTomorrowCount { get; set; }
    }
}
