using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class MostRecentActiveUserDto
    {
        public string TenancyName { get; set; }
        public string FullName { get; set; }
        public DateTime LastTransaction { get; set; }
        public int NumberOfTransactions { get; set; }

    }
}
