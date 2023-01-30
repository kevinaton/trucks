using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class LuckStoneSettingsEditDto
    {
        public bool AllowImportingLuckStoneEarnings { get; set; }
        public int? LuckStoneCustomerId { get; set; }
        public string LuckStoneCustomerName { get; set; }
        public string HaulerRef { get; set; }
        public bool UseForProductionPay { get; set; }
    }
}
