using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Receipts.Dto
{
    public class EditReceiptLineOutput
    {
        public int ReceiptLineId { get; set; }
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
