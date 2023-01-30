using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ReassignTrucksInput
    {
        public int SourceOrderLineId { get; set; }
        public int DestinationOrderLineId { get; set; }
        public int[] TruckIds { get; set; }
    }
}
