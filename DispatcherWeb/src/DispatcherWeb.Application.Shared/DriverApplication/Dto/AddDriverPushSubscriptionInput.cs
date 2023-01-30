using DispatcherWeb.WebPush;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class AddDriverPushSubscriptionInput
    {
        public PushSubscriptionDto PushSubscription { get; set; }
        public int DriverId { get; set; }
        public int? DeviceId { get; set; }
    }
}
