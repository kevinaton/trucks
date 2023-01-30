using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Projects
{
    [Table("ProjectHistory")]
    public class ProjectHistoryRecord : FullAuditedEntity, IMustHaveTenant
	{
		public int TenantId { get; set; }

		public int ProjectId { get; set; }
        public DateTime DateTime { get; set; }
        public long? UserId { get; set; }
        public int? OfficeId { get; set; }
        public ProjectHistoryAction Action { get; set; }

        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
        public virtual Office Office { get; set; }

    }
}
