using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineDeliverToIdInput
    {
        public int OrderLineId { get; set; }
        public int? DeliverToId { get; set; }
    }
}
