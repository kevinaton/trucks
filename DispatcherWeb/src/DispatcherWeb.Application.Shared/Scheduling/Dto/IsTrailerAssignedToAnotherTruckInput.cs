using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class IsTrailerAssignedToAnotherTruckInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public int TrailerId { get; set; }
        public int ParentTruckId { get; set; }
    }
}
