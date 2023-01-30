using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class CreateInvoicesForTicketsInput
    {
        public List<int> TicketIds { get; set; }
    }
}
