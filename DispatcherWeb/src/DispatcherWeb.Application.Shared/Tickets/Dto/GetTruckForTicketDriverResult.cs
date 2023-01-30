using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetTruckForTicketDriverResult
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
    }
}
