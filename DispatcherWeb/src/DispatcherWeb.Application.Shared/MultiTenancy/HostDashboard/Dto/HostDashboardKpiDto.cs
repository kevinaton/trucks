using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class HostDashboardKpiDto
    {
        public int ActiveTenants { get; set; }
        public int ActiveTrucks { get; set; }
        public int ActiveUsers { get; set; }
        public int UsersWithActivity { get; set; }
        public int OrderLinesCreated { get; set; }
        public int InternalTrucksScheduled { get; set; }
        public int InternalScheduledDeliveries { get; set; }
        public int LeaseHaulerScheduledDeliveries { get; set; }
        public int TicketsCreated { get; set; }
        public int SmsSent { get; set; }
    }
}
