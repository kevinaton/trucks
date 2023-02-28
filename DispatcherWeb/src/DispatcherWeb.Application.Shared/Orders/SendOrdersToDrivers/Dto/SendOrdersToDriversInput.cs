using System;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.Dto
{
    public class SendOrdersToDriversInput
    {
        public DateTime DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
    }
}
