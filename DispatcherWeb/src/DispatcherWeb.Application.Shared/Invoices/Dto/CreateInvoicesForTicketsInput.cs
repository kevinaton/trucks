﻿using System.Collections.Generic;

namespace DispatcherWeb.Invoices.Dto
{
    public class CreateInvoicesForTicketsInput
    {
        public List<int> TicketIds { get; set; }
    }
}
