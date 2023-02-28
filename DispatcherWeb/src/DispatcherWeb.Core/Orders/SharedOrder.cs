using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Orders
{
    [Table("SharedOrder")]
    public class SharedOrder : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int OrderId { get; set; }
        public int OfficeId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Office Office { get; set; }
    }
}
