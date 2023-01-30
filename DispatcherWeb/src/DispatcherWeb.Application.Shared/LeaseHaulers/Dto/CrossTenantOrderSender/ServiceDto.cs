using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class ServiceDto
    {
        public string Service1 { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public ServiceType? Type { get; set; }
    }
}
