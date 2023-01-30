using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.VehicleMaintenance
{
	[Table("WorkOrderPicture")]
    public class WorkOrderPicture : FullAuditedEntity, IMustHaveTenant
	{
		public int TenantId { get; set; }

		public int WorkOrderId { get; set; }
		public WorkOrder WorkOrder { get; set; }

		public Guid FileId { get; set; }

		public string FileName { get; set; }
	}
}
