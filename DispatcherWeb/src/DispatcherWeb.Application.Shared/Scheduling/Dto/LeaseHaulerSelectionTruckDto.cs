using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class LeaseHaulerSelectionTruckDto
    {
        public int LeaseHaulerId { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DefaultDriverId { get; set; }
    }
}
