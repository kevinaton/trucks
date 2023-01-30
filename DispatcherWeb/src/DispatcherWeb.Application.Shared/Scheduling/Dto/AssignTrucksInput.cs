using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class AssignTrucksInput
    {
        public int OrderLineId { get; set; }
        public List<int> TruckIds { get; set; }
    }
}
