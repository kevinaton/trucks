using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.TimeOffs.Dto
{
    public class RemoveDriverFromTrucksInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DriverId { get; set; }
    }
}
