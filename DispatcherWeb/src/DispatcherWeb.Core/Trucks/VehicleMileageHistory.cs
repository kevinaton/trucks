﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Trucks
{
    [Table("VehicleMileageHistory")]
    public class VehicleMileageHistory : FullAuditedEntity, IMustHaveTenant
	{
		public int TenantId { get; set; }

		public int TruckId { get; set; }
		public Truck Truck { get; set; }

		public DateTime DateTime { get; set; }

		public int Mileage { get; set; }

		public decimal Hours { get; set; }
	}
}
