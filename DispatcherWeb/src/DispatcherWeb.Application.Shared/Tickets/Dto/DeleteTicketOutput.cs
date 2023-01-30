using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class DeleteTicketOutput
    {
        public IOrderTaxDetailsWithActualAmounts OrderTaxDetails { get; set; }
    }
}
