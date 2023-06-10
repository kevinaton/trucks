using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using DispatcherWeb.VehicleCategories;

namespace DispatcherWeb.Orders
{
    [Table("OrderLineVehicleCategory")]
    public class OrderLineVehicleCategory : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int OrderLineId { get; set; }
        public virtual OrderLine OrderLine { get; set; }

        public int VehicleCategoryId { get; set; }
        public virtual VehicleCategory VehicleCategory { get; set; }
    }
}
