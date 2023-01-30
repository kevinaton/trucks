using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class EditOrderTicketOutput
    {
        public OrderTicketEditDto Ticket { get; set; }
        public IOrderTaxDetailsWithActualAmounts OrderTaxDetails { get; set; }
        public string WarningText { get; set; }
    }
}
