using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Receipts.Dto
{
    public class DeleteReceiptLineOutput
    {
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
