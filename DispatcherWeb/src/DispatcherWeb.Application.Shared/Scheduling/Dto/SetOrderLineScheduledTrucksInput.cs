using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineScheduledTrucksInput
    {
        public int OrderLineId { get; set; }
        public double? ScheduledTrucks { get; set; }
    }
}
