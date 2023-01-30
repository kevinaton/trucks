using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.Dto
{
    public class IsOrderLineFieldReadonlyInput
    {
        public int OrderLineId { get; set; }
        public string FieldName { get; set; }
    }
}
