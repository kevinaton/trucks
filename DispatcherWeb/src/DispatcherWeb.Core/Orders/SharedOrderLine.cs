using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Orders
{
    [Table("SharedOrderLine")]
    public class SharedOrderLine : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int OrderLineId { get; set; }
        public virtual OrderLine OrderLine { get; set; }

        public int OfficeId { get; set; }
        public virtual Office Office { get; set; }

    }
}
