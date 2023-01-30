using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders
{
    [Table("OrderLineOfficeAmount")]
    public class OrderLineOfficeAmount : FullAuditedEntity, IMustHaveTenant
	{
		public int TenantId { get; set; }

		public int OrderLineId { get; set; }
        public int OfficeId { get; set; }

        public decimal? ActualQuantity { get; set; }

        public virtual OrderLine OrderLine { get; set; }
        public virtual Office Office { get; set; }
    }
}
