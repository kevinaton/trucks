using DispatcherWeb.Orders.TaxDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoiceLineEditDto : IOrderLineTaxTotalDetails
    {
        public int? Id { get; set; }

        public short LineNumber { get; set; }

        public int? TicketId { get; set; }

        public string TicketNumber { get; set; }

        public string JobNumber { get; set; }

        public string PoNumber { get; set; }

        public int? CarrierId { get; set; }

        public string CarrierName { get; set; }

        public DateTime? DeliveryDateTime { get; set; }

        [StringLength(25)]
        public string TruckCode { get; set; }

        public string LeaseHaulerName { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal? MaterialRate { get; set; }

        public decimal? FreightRate { get; set; }

        public int? ItemId { get; set; }

        public string ItemName { get; set; }

        public decimal MaterialExtendedAmount { get; set; }

        public decimal FreightExtendedAmount { get; set; }

        decimal IOrderLineTaxDetails.MaterialPrice => MaterialExtendedAmount;

        decimal IOrderLineTaxDetails.FreightPrice => FreightExtendedAmount;

        public bool? IsTaxable { get; set; }

        public decimal Tax { get; set; }

        public decimal FuelSurcharge { get; set; }

        public decimal Subtotal { get; set; }

        public decimal ExtendedAmount { get; set; }

        public Guid? Guid { get; set; }

        public Guid? ParentInvoiceLineGuid { get; set; }

        public int? ParentInvoiceLineId { get; set; }

        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }

        decimal IOrderLineTaxTotalDetails.TotalAmount { get => ExtendedAmount; set => ExtendedAmount = value; }
        bool IOrderLineTaxDetails.IsTaxable => IsTaxable ?? true; //true if no service/ticket
    }
}
