using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets.Dto
{
    public class DeleteTicketOutput
    {
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
