using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.Dto
{
    public class EditOrderLineOfficeAmountOutput
    {
        public IOrderTaxDetailsWithActualAmounts OrderTaxDetails { get; set; }
    }
}
