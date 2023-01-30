using Abp.Domain.Entities.Auditing;
using DispatcherWeb.WebPush;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Drivers
{
    [Table("DriverPushSubscription")]
    public class DriverPushSubscription : FullAuditedEntity
    {
        public int DriverId { get; set; }
        public int? DeviceId { get; set; }
        public int PushSubscriptionId { get; set; }
        public virtual Driver Driver { get; set; }
        public virtual DriverApplicationDevice Device { get; set; }
        public virtual PushSubscription PushSubscription { get;set; }
    }
}
