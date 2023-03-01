using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Orders
{
    [Table("OrderTruck")]
    public class OrderTruck : FullAuditedEntity, IMustHaveTenant
    {
        public OrderTruck()
        {
            DependentOrderTrucks = new HashSet<OrderTruck>();
        }

        public int TenantId { get; set; }

        public int OrderId { get; set; }

        public int TruckId { get; set; }

        public decimal Utilization { get; set; }

        public virtual Order Order { get; set; }

        public virtual Truck Truck { get; set; }

        public virtual int? ParentOrderTruckId { get; set; }

        public virtual OrderTruck ParentOrderTruck { get; set; }

        public virtual ICollection<OrderTruck> DependentOrderTrucks { get; set; }
    }
}
