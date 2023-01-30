using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class SaveWorkOrderLineResult
    {
        public decimal TotalLaborCost { get; set; }
        public decimal TotalPartsCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
