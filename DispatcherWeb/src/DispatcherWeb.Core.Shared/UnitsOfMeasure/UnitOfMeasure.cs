using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.UnitsOfMeasure
{
    [Table("UnitOfMeasure")]
    public class UnitOfMeasure : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        //public override string ToString()
        //{
        //    return Name;
        //}
    }
}
