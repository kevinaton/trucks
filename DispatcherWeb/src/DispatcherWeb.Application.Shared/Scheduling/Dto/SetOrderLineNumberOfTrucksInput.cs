using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineNumberOfTrucksInput
    {
        public int OrderLineId { get; set; }
        public double? NumberOfTrucks { get; set; }

    }
}
