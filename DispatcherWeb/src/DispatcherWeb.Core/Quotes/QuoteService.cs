using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Quotes
{
    [Table("QuoteService")]
    public class QuoteService : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public int QuoteId { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        [Column(TypeName = "money")]
        public decimal? PricePerUnit { get; set; }

        [Column(TypeName = "money")]
        public decimal? FreightRate { get; set; }

        public decimal? LeaseHaulerRate { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public virtual UnitOfMeasure MaterialUom { get; set; }

        public virtual UnitOfMeasure FreightUom { get; set; }

        public virtual Quote Quote { get; set; }

        public virtual Service Service { get; set; }

        public virtual Location LoadAt { get; set; }

        public virtual Location DeliverTo { get; set; }

        public QuoteService Clone()
        {
            return new QuoteService
            {
                CreationTime = CreationTime,
                CreatorUserId = CreatorUserId,
                DeleterUserId = DeleterUserId,
                DeletionTime = DeletionTime,
                DeliverToId = DeliverToId,
                Designation = Designation,
                FreightQuantity = FreightQuantity,
                FreightRate = FreightRate,
                FreightUomId = FreightUomId,
                Id = Id,
                IsDeleted = IsDeleted,
                JobNumber = JobNumber,
                LastModificationTime = LastModificationTime,
                LastModifierUserId = LastModifierUserId,
                LeaseHaulerRate = LeaseHaulerRate,
                LoadAtId = LoadAtId,
                MaterialQuantity = MaterialQuantity,
                MaterialUomId = MaterialUomId,
                Note = Note,
                PricePerUnit = PricePerUnit,
                QuoteId = QuoteId,
                ServiceId = ServiceId,
                TenantId = TenantId,
            };
        }
    }
}
