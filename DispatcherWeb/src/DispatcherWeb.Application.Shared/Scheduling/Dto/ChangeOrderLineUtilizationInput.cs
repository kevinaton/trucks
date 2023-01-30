using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ChangeOrderLineUtilizationInput
    {
        public int OrderLineId { get; set; }
        public decimal Utilization { get; set; }
    }
}
