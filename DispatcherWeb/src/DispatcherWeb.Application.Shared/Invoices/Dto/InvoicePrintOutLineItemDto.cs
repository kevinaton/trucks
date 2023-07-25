using System;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoicePrintOutLineItemDto
    {
        public int Id { get; set; }
        public short? LineNumber { get; set; }
        public string TicketNumber { get; set; }
        public string LeaseHaulerName { get; set; }
        public string TruckCode { get; set; }
        public DateTime? DeliveryDateTime { get; set; }

        public string Description { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ExtendedAmount { get; set; }
        public decimal MaterialExtendedAmount { get; set; }
        public decimal FreightExtendedAmount { get; set; }
        public decimal Tax { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? MaterialRate { get; set; }
        public decimal? RateSum => (FreightRate ?? 0) + (MaterialRate ?? 0);
        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }
        public int? ParentInvoiceLineId { get; set; }
        public string JobNumber { get; set; }
    }
}
