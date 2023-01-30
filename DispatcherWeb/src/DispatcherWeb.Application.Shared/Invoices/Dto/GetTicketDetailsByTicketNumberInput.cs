using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetTicketDetailsByTicketNumberInput
    {
        public string TicketNumber { get; set; }
        public int CustomerId { get; set; }
    }
}
