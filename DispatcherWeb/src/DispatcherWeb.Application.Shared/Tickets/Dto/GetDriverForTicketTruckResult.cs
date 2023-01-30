using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetDriverForTicketTruckResult
    {
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
        public bool TruckCodeIsCorrect { get; set; }
    }
}
