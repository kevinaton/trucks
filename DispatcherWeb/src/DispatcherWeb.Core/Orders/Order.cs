using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Customers;
using DispatcherWeb.Emailing;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Payments;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.Orders
{
    [Table("Order")]
    public class Order : FullAuditedEntity, IMustHaveTenant, IOrderTaxDetailsWithActualAmounts
    {
        public Order()
        {
            OrderLines = new List<OrderLine>();
            SharedOrders = new HashSet<SharedOrder>();
            BilledOrders = new HashSet<BilledOrder>();
            OrderEmails = new HashSet<OrderEmail>();
            Receipts = new HashSet<Receipt>();
        }

        public int TenantId { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public Shift? Shift { get; set; }

        public bool IsPending { get; set; }

        public DateTime? SharedDateTime { get; set; }

        [Required(ErrorMessage = "Customer is a required field")]
        public int CustomerId { get; set; }

        [StringLength(EntityStringFieldLengths.Order.PoNumber)]
        public string PONumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.SpectrumNumber)]
        public string SpectrumNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.JobNumber)]
        public string JobNumber { get; set; }

        public int? ContactId { get; set; }

        [Column(TypeName = "money")]
        public decimal SalesTaxRate { get; set; }

        [Column(TypeName = "money")]
        public decimal SalesTax { get; set; }

        [Column(TypeName = "money")]
        public decimal CODTotal { get; set; }

        public bool HasAllActualAmounts { get; set; }

        [StringLength(EntityStringFieldLengths.Order.ChargeTo)]
        public string ChargeTo { get; set; }

        public decimal FreightTotal { get; set; }

        public decimal MaterialTotal { get; set; }

        public bool IsFreightTotalOverridden { get; set; }

        public bool IsMaterialTotalOverridden { get; set; }

        [StringLength(EntityStringFieldLengths.Order.Directions)]
        public string Directions { get; set; }

        public string EncryptedInternalNotes { get; set; }

        public bool HasInternalNotes { get; set; }

        [Obsolete("TODO check if we are still using this NumberOfTrucks in any meaningful way")]
        public double? NumberOfTrucks { get; set; }

        [Required(ErrorMessage = "Office is a required field")]
        public int LocationId { get; set; }

        public int? ProjectId { get; set; }

        public int? QuoteId { get; set; }

        public bool IsClosed { get; set; }

        public Guid? LastQuoteEmailId { get; set; }

        public OrderPriority Priority { get; set; }

        public bool IsImported { get; set; }

        public virtual TrackableEmail LastQuoteEmail { get; set; }

        /// <summary>
        /// MaterialCompany's order id. Only set for HaulingCompany orders when a copy of this order exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyOrderId { get; set; }
        /// <summary>
        /// MaterialCompany's tenant id. Only set for HaulingCompany orders when a copy of this order exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyTenantId { get; set; }

        //those ids are commented out since one  MaterialCompany order can be associated with multiple HaulingCompany orders, one per order line
        // <summary>
        // HaulingCompany's order id. Only set for MaterialCompany orders when a copy of this order exists on another HaulingCompany tenant.
        // </summary>
        //public int? HaulingCompanyOrderId { get; set; }
        // <summary>
        // HaulingCompany's tenant id. Only set for MaterialCompany orders when a copy of this order exists on another HaulingCompany tenant.
        // </summary>
        //public int? HaulingCompanyTenantId { get; set; }

        //This will be set only for MaterialCompany's orders.
        public bool HasLinkedHaulingCompanyOrders { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("ContactId")]
        public virtual CustomerContact CustomerContact { get; set; }

        [ForeignKey("LocationId")]
        public virtual Office Office { get; set; }

        public virtual Project Project { get; set; }

        public virtual Quote Quote { get; set; }

        public virtual User CreatorUser { get; set; }

        public virtual User LastModifierUser { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public virtual ICollection<SharedOrder> SharedOrders { get; set; }

        public virtual ICollection<BilledOrder> BilledOrders { get; set; }

        public virtual ICollection<OrderEmail> OrderEmails { get; set; }

        public virtual ICollection<OrderPayment> OrderPayments { get; set; }

        public virtual ICollection<Receipt> Receipts { get; set; }

        [Column(TypeName = "money")]
        public decimal? BaseFuelCost { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public virtual FuelSurchargeCalculation FuelSurchargeCalculation { get; set; }
    }
}
