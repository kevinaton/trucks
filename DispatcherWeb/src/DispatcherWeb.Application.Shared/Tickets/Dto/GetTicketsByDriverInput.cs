using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetTicketsByDriverInput
    {
        public DateTime Date { get; set; }
        public int? DriverId { get; set; }


    }
}
