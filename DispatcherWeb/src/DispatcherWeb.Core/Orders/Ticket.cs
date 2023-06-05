﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Invoices;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulerStatements;
using DispatcherWeb.Locations;
using DispatcherWeb.Offices;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Services;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Orders
{
    [Table("Ticket")]
    public class Ticket : FullAuditedEntity, IMustHaveTenant
    {

        public int TenantId { get; set; }

        public int? OrderLineId { get; set; }
        [ForeignKey("OrderLineId")]
        public OrderLine OrderLine { get; set; }

        public int? ReceiptLineId { get; set; }

        public virtual ReceiptLine ReceiptLine { get; set; }

        public int? OfficeId { get; set; }
        public virtual Office Office { get; set; }

        [StringLength(EntityStringFieldLengths.Ticket.TicketNumber)]
        public string TicketNumber { get; set; }

        public decimal Quantity { get; set; }

        public int? TruckId { get; set; }
        [ForeignKey("TruckId")]
        public virtual Truck Truck { get; set; }
        [StringLength(Truck.MaxTruckCodeLength)]
        public string TruckCode { get; set; }

        public int? TrailerId { get; set; }
        public virtual Truck Trailer { get; set; }

        public int? CarrierId { get; set; }
        [ForeignKey("CarrierId")]
        public virtual LeaseHauler Carrier { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public int? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        public int? LoadAtId { get; set; }

        public virtual Location LoadAt { get; set; }

        public int? DeliverToId { get; set; }

        public virtual Location DeliverTo { get; set; }

        public int? UnitOfMeasureId { get; set; }

        [ForeignKey("UnitOfMeasureId")]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        public DateTime? TicketDateTime { get; set; }

        public Shift? Shift { get; set; }

        public bool IsBilled { get; set; }

        public bool IsVerified { get; set; }

        public Guid? TicketPhotoId { get; set; }

        public Guid? DeferredTicketPhotoId { get; set; }

        [StringLength(255)]
        public string TicketPhotoFilename { get; set; }

        public bool IsImported { get; set; }

        [Column(TypeName = "money")]
        public decimal FuelSurcharge { get; set; }

        //public int? InvoiceLineId { get; set; }

        public virtual InvoiceLine InvoiceLine { get; set; }

        public int? LoadId { get; set; }

        public virtual Load Load { get; set; }

        public int? DriverId { get; set; }

        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        /// <summary>
        /// MaterialCompany's ticket id. Only set for HaulingCompany tickets when a copy of this ticket exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyTicketId { get; set; }
        /// <summary>
        /// MaterialCompany's tenant id. Only set for HaulingCompany tickets when a copy of this ticket exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyTenantId { get; set; }

        /// <summary>
        /// HaulingCompany's ticket id. Only set for MaterialCompany tickets when a copy of this ticket exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTicketId { get; set; }
        /// <summary>
        /// HaulingCompany's tenant id. Only set for MaterialCompany tickets when a copy of this ticket exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        public virtual LeaseHaulerStatementTicket LeaseHaulerStatementTicket { get; set; }

        public virtual ICollection<PayStatementTicket> PayStatementTickets { get; set; }

    }
}
