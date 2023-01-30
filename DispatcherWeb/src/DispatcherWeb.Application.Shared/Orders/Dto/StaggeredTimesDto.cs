using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.Dto
{
    public class StaggeredTimesDto
    {
        public int? OrderLineId { get; set; }
        public StaggeredTimeKind StaggeredTimeKind { get; set; }
        public int? StaggeredTimeInterval { get; set; }
        public DateTime? FirstStaggeredTimeOnJob { get; set; }
    }
}
