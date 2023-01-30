using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.TaxDetails
{
    public interface IOrderTaxDetailsWithActualAmounts : IOrderTaxDetails
    {
        bool HasAllActualAmounts { get; set; }
    }
}
