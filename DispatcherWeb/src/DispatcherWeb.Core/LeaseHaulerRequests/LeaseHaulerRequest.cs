using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [Table("LeaseHaulerRequest")]
    public class LeaseHaulerRequest : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public Guid Guid { get; set; }

        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public int OfficeId { get; set; }
        public Office Office { get; set; }

        public int LeaseHaulerId { get; set; }
        public LeaseHauler LeaseHauler { get; set; }

        public DateTime? Sent { get; set; }

        public int? Available { get; set; }
        public int? Approved { get; set; }

        [StringLength(EntityStringFieldLengths.General.Length2000)]
        public string Comments { get; set; }
    }
}
