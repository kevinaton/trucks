using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.Dto
{
    public class SendOrdersToDriversInput
    {
		public DateTime DeliveryDate { get; set; }
		public Shift? Shift { get; set; }
    }
}
