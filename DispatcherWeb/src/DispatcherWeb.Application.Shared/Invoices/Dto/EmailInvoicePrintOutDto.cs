using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class EmailInvoicePrintOutDto
    {
        public int InvoiceId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
