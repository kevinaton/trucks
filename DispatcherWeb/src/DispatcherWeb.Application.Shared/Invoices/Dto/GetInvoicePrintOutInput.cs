using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetInvoicePrintOutInput
    {
        public int InvoiceId { get; set; }
        public bool DebugLayout { get; set; }
    }
}
