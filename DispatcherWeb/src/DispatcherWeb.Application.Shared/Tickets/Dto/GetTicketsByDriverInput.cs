using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetTicketsByDriverInput
    {
        public DateTime Date { get; set; }
        public int? DriverId { get; set; }


    }
}
