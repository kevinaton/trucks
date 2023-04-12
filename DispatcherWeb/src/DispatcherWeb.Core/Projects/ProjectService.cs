using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Locations;
using DispatcherWeb.Services;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Projects
{
    [Table("ProjectService")]
    public class ProjectService : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public int ProjectId { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        [Column(TypeName = "money")]
        public decimal? PricePerUnit { get; set; }

        [Column(TypeName = "money")]
        public decimal? FreightRate { get; set; }

        [Column(TypeName = "money")]
        public decimal? LeaseHaulerRate { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public virtual UnitOfMeasure MaterialUom { get; set; }

        public virtual UnitOfMeasure FreightUom { get; set; }

        public virtual Project Project { get; set; }

        public virtual Service Service { get; set; }

        public virtual Location LoadAt { get; set; }

        public virtual Location DeliverTo { get; set; }
    }
}
