using System;
using System.Collections.Generic;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class BillingReconciliationDto : BillingReconciliationDto<BillingReconciliationItemDto>
    {
    }

    public class BillingReconciliationDto<T> : IOrderTaxDetails where T : BillingReconciliationItemDto
    {
        public int Id { get; set; }
        public bool IsBilled { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string CustomerName { get; set; }
        public decimal SalesTaxRate { get; set; }
        public List<T> Items { get; set; }
        public decimal FreightTotal { get; set; }
        public decimal MaterialTotal { get; set; }
        public decimal SalesTax { get; set; }
        public decimal CODTotal { get; set; }
        public bool IsShared { get; set; }
    }
}
