using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UpdateDispatchTicketInput
    {
        public DispatchTicketDto DispatchTicket { get; set; }
        public DriverApplicationActionInfo Info { get; set; }
    }
}
