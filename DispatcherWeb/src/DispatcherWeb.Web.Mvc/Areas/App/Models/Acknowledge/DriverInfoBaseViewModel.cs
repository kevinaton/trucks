using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.Areas.App.Models.Acknowledge
{
    public class DriverInfoBaseViewModel
    {
        public Guid Guid { get; set; }
        public int DispatchId { get; set; }
        public int TenantId { get; set; }
    }
}
