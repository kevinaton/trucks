using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class TruxSettingsEditDto
    {
        public bool AllowImportingTruxEarnings { get; set; }
        public int? TruxCustomerId { get; set; }
        public string TruxCustomerName { get; set; }
        public bool UseForProductionPay { get; set; }
    }
}
