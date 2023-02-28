using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Invoices
{
    [Table("InvoiceLine")]
    public class InvoiceLine : FullAuditedEntity, IMustHaveTenant
    {
        public const int MaxDescriptionLength = 1000;

        public int TenantId { get; set; }

        public int InvoiceId { get; set; }

        public virtual Invoice Invoice { get; set; }

        public int? ParentInvoiceLineId { get; set; }

        public virtual InvoiceLine ParentInvoiceLine { get; set; }

        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }

        public int? ItemId { get; set; }

        public virtual Service Item { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        public short LineNumber { get; set; }

        public int? TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }

        public int? CarrierId { get; set; }

        public virtual LeaseHauler Carrier { get; set; }

        public DateTime? DeliveryDateTime { get; set; }

        [StringLength(Truck.MaxTruckCodeLength)]
        public string TruckCode { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal? MaterialRate { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal MaterialExtendedAmount { get; set; }

        public decimal FreightExtendedAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal FuelSurcharge { get; set; }

        public decimal Tax { get; set; }

        public decimal Subtotal { get; set; }

        public decimal ExtendedAmount { get; set; }
    }
}
