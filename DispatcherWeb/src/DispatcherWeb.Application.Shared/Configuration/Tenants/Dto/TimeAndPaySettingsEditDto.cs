using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class TimeAndPaySettingsEditDto
    {
        public int TimeTrackingDefaultTimeClassificationId { get; set; }
        public string TimeTrackingDefaultTimeClassificationName { get; set; }
        public bool AllowProductionPay { get; set; }
        public bool DefaultToProductionPay { get; set; }
        public bool PreventProductionPayOnHourlyJobs { get; set; }
    }
}
