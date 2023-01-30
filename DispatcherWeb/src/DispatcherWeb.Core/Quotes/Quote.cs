﻿using System;
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
using DispatcherWeb.Orders;
using DispatcherWeb.Projects;

namespace DispatcherWeb.Quotes
{
    [Table("Quote")]
    public class Quote : FullAuditedEntity, IMustHaveTenant
    {
        public const int MaxNameLength = 155;

        public Quote()
        {
            QuoteServices = new HashSet<QuoteService>();
            QuoteEmails = new HashSet<QuoteEmail>();
        }

        public int TenantId { get; set; }

        public int? ProjectId { get; set; }

        public int CustomerId { get; set; }

        public int? ContactId { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime? ProposalDate { get; set; }

        public DateTime? ProposalExpiryDate { get; set; }

        public DateTime? InactivationDate { get; set; }

        [StringLength(20)]
        public string PONumber { get; set; }

        [StringLength(20)]
        public string SpectrumNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.JobNumber)]
        public string JobNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal? BaseFuelCost { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public virtual FuelSurchargeCalculation FuelSurchargeCalculation { get; set; }

        [StringLength(500)]
        public string ChargeTo { get; set; }

        public string Directions { get; set; }

        public string Notes { get; set; }

        public bool CaptureHistory { get; set; }

        public ProjectStatus Status { get; set; }

        public long? SalesPersonId { get; set; }

        public Guid? LastQuoteEmailId { get; set; }

        public virtual TrackableEmail LastQuoteEmail { get; set; }

        public virtual Project Project { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual CustomerContact Contact { get; set; }

        public virtual User CreatorUser { get; set; }

        public virtual User SalesPerson { get; set; }

        public virtual ICollection<QuoteService> QuoteServices { get; set; }

        public virtual ICollection<QuoteEmail> QuoteEmails { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
