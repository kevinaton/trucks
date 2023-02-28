using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class BillingReconciliationItemDto : IOrderLineTaxDetails
    {
        public decimal? TicketQuantity { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public bool IsTaxable { get; set; }
        public decimal MaterialPrice => IsOrderLineMaterialPriceOverridden
            ? OrderLineMaterialPrice
            : decimal.Round((MaterialPricePerUnit ?? 0) * (TicketQuantity ?? 0), 2);
        public decimal FreightPrice => IsOrderLineFreightPriceOverridden
            ? OrderLineFreightPrice
            : decimal.Round((FreightPricePerUnit ?? 0) * (TicketQuantity ?? 0), 2);
        public decimal OrderLineMaterialPrice { get; set; }
        public decimal OrderLineFreightPrice { get; set; }
        public bool IsOrderLineMaterialPriceOverridden { get; set; }
        public bool IsOrderLineFreightPriceOverridden { get; set; }
    }
}
