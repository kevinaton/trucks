using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class AddTicketPhotoInput
    {
        public int TicketId { get; set; }
        public string TicketPhoto { get; set; }
        public string TicketPhotoFilename { get; set; }
    }
}
