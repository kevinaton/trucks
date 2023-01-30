using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class MoveTruckInput
    {
        public int TruckId { get; set; }
        public int SourceOrderLineTruckId { get; set; }
        public int DestinationOrderLineId { get; set; }
    }
}
