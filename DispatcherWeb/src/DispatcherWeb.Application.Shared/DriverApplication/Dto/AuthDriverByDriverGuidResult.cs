using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class AuthDriverByDriverGuidResult
    {
        public int TenantId { get; set; }
        public long UserId { get; set; }
        public int DriverId { get; set; }
    }
}
