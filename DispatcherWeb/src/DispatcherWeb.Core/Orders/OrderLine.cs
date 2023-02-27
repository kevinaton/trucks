using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Locations;
using DispatcherWeb.Quotes;
using DispatcherWeb.Services;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Orders
{
    [Table("OrderLine")]
    public class OrderLine : FullAuditedEntity, IMustHaveTenant
    {
        public const int MaxPickupNoteLength = 400;
        public const int MaxDeliveryNoteLength = 400;

        public OrderLine()
        {
            OfficeAmounts = new HashSet<OrderLineOfficeAmount>();
            OrderLineTrucks = new HashSet<OrderLineTruck>();
            SharedOrderLines = new HashSet<SharedOrderLine>();
            Tickets = new HashSet<Ticket>();
            Dispatches = new HashSet<Dispatch>();
            ReceiptLines = new HashSet<ReceiptLine>();
        }

        public int TenantId { get; set; }

        public int OrderId { get; set; }

        public bool IsComplete { get; set; }

        public bool IsCancelled { get; set; }

        public int LineNumber { get; set; }

        public DateTime? SharedDateTime { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialPricePerUnit { get; set; }

        public decimal? FreightPricePerUnit { get; set; }

        public bool IsMaterialPricePerUnitOverridden { get; set; }

        public bool IsFreightPricePerUnitOverridden { get; set; }
        
        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        public int? MaterialUomId { get; set; }

        public virtual UnitOfMeasure MaterialUom { get; set; }

        public int? FreightUomId { get; set; }

        public virtual UnitOfMeasure FreightUom { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        [Column(TypeName = "money")]
        public decimal MaterialPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal MaterialActualPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal FreightPrice { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        [Column(TypeName = "money")]
        public decimal? FuelSurchargeRate { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        //Todo
        //[StringLength(EntityStringFieldLengths.OrderLine.DriverNote)]
        //public string DriverNote { get; set; }

        public int? Loads { get; set; }

        public decimal? EstimatedAmount { get; set; }

        [Obsolete]
        public DateTime? TimeOnJobObsolete { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public StaggeredTimeKind StaggeredTimeKind { get; set; }

        public int? StaggeredTimeInterval { get; set; } //in minutes

        [Obsolete]
        public DateTime? FirstStaggeredTimeOnJobObsolete { get; set; }

        public DateTime? FirstStaggeredTimeOnJob { get; set; }

        public double? NumberOfTrucks { get; set; }

        public double? ScheduledTrucks { get; set; }

        public bool IsMultipleLoads { get; set; }

        /// <summary>
        /// MaterialCompany's order line id. Only set for HaulingCompany order lines when a copy of this order line exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyOrderLineId { get; set; }
        /// <summary>
        /// MaterialCompany's tenant id. Only set for HaulingCompany order lines when a copy of this order line exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyTenantId { get; set; }

        /// <summary>
        /// HaulingCompany's order line id. Only set for MaterialCompany order lines when a copy of this order line exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyOrderLineId { get; set; }
        /// <summary>
        /// HaulingCompany's tenant id. Only set for MaterialCompany order lines when a copy of this order line exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        public int? QuoteServiceId { get; set; }

        public virtual QuoteService QuoteService { get; set; }

        public virtual Order Order { get; set; }

        public virtual Location LoadAt { get; set; }

        public virtual Location DeliverTo { get; set; }

        public virtual Service Service { get; set; }

        public bool HasAllActualAmounts { get; set; }

        public bool ProductionPay { get; set; }

        public virtual ICollection<OrderLineOfficeAmount> OfficeAmounts { get; set; }

        public virtual ICollection<OrderLineTruck> OrderLineTrucks { get; set; }

        public virtual ICollection<SharedOrderLine> SharedOrderLines { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

        public virtual ICollection<Dispatch> Dispatches { get; set; }

        public virtual ICollection<ReceiptLine> ReceiptLines { get; set; }
    }
}
