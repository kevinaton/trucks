using System;

namespace DispatcherWeb.Web.Areas.App.Models.Acknowledge
{
    public class DriverInfoBaseViewModel
    {
        public Guid Guid { get; set; }
        public int DispatchId { get; set; }
        public int TenantId { get; set; }
    }
}
