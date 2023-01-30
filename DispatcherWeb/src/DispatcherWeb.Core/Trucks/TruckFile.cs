using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Trucks
{
    [Table("TruckFile")]
    public class TruckFile : FullAuditedEntity, IMustHaveTenant
	{
		public int TenantId { get; set; }

		public int TruckId { get; set; }
		public Truck Truck { get; set; }

		[StringLength(50)]
		public string Title { get; set; }
		[StringLength(1000)]
		public string Description { get; set; }

		public Guid FileId { get; set; }
		public Guid? ThumbnailId { get; set; }

		[StringLength(500)]
		public string FileName { get; set; }

		public FileType FileType { get; set; }
	}
}
