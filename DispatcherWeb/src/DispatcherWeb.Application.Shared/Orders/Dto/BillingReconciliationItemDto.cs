using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class BillingReconciliationItemDto : IOrderLineTaxDetails
    {
        public bool AllowAddingTickets { get; set; }
        public decimal? TicketQuantity { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        //public decimal ReceiptLinesMaterialTotal { get; set; }
        //public decimal ReceiptLinesFreightTotal { get; set; }
        public bool IsTaxable { get; set; }
        public decimal MaterialPrice => IsOrderLineMaterialPriceOverridden
            ? OrderLineMaterialPrice
            : AllowAddingTickets
                ? decimal.Round((MaterialPricePerUnit ?? 0) * (TicketQuantity ?? 0), 2)
                : 0; //decimal.Round(ReceiptLinesMaterialTotal, 2);
        public decimal FreightPrice => IsOrderLineFreightPriceOverridden
            ? OrderLineFreightPrice
            : AllowAddingTickets
                ? decimal.Round((FreightPricePerUnit ?? 0) * (TicketQuantity ?? 0), 2)
                : 0; //decimal.Round(ReceiptLinesFreightTotal, 2);
        public decimal OrderLineMaterialPrice { get; set; }
        public decimal OrderLineFreightPrice { get; set; }
        public bool IsOrderLineMaterialPriceOverridden { get; set; }
        public bool IsOrderLineFreightPriceOverridden { get; set; }
    }
}
