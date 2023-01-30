using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.Dto
{
    public class ReceiptDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public DateTime ReceiptDate { get; set; }
    }
}
