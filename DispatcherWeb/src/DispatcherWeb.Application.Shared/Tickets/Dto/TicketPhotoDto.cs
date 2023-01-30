using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketPhotoDto
    {
        public byte[] FileBytes { get; set; }
        public string Filename { get; set; }
    }
}
