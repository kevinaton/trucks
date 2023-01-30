using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Trucks
{
    [Table("SharedTruck")]
    public class SharedTruck : FullAuditedEntity, IMustHaveTenant
	{
		public int TenantId { get; set; }

		public int TruckId { get; set; }
        public int OfficeId { get; set; }
        public DateTime Date { get; set; }
		public Shift? Shift { get; set; }
        public virtual Truck Truck { get; set; }
        public virtual Office Office { get; set; }
    }
}
