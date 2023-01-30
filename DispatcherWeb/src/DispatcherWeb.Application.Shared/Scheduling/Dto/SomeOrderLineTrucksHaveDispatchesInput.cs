using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SomeOrderLineTrucksHaveDispatchesInput
    {
        public int OrderLineId { get; set; }
        public int[] TruckIds { get; set; }
    }
}
