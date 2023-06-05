using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Trucks
{
    [Table("TrailerAssignment")]
    public class TrailerAssignment : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }
        
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        
        public int? OfficeId { get; set; }
        public virtual Office Office { get; set; }
        
        public int TractorId { get; set; }
        public virtual Truck Tractor { get; set; }

        public int? TrailerId { get; set; }
        public virtual Truck Trailer { get; set; }
    }
}
