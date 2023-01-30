using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.TaxDetails
{
    public interface IOrderLineTaxDetailsWithMultipleActualAmounts : IOrderLineTaxDetails
    {
        bool HasAllActualAmounts { get; set; }
    }
}
