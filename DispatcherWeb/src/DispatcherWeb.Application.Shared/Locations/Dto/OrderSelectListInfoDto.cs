using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Locations.Dto
{
    public class OrderSelectListInfoDto
    {
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public LocationSelectListInfoDto DeliverTo { get; set; }
    }
}
