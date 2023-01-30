using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Trucks.Dto
{
    public class SharedTruckDto
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public int TruckId { get; set; }
    }
}
