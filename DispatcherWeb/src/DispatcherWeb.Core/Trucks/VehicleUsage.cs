using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Trucks
{
    [Table("VehicleUsage")]
    public class VehicleUsage : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public DateTime ReadingDateTime { get; set; }
        public ReadingType ReadingType { get; set; }
        public decimal Reading { get; set; }

    }
}
