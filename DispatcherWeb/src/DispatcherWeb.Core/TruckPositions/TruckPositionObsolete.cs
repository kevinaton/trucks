using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.TruckPositions
{
    [Obsolete]
    [Table("TruckPosition")]
    public class TruckPositionObsolete : CreationAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int? TruckId { get; set; }

        public int DriverId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? Accuracy { get; set; }

        public decimal? Speed { get; set; }

        public decimal? Heading { get; set; }

        public decimal? Altitude { get; set; }

        public TruckPositionActivityType ActivityType { get; set; }

        [StringLength(EntityStringFieldLengths.General.Medium)]
        public string ActivitiTypeRaw { get; set; }

        public int? ActivityConfidence { get; set; }

        [StringLength(EntityStringFieldLengths.TruckPosition.GeofenceIdentifier)]
        public string GeofenceIdentifier { get; set; }

        public TruckPositionGeofenceAction? GeofenceAction { get; set; }

        [StringLength(EntityStringFieldLengths.General.Medium)]
        public string GeofenceActionRaw { get; set; }

        public decimal? BatteryLevel { get; set; }

        public bool? BatteryIsCharging { get; set; }

        public DateTime Timestamp { get; set; }

        public Guid Uuid { get; set; }

        public TruckPositionEvent Event { get; set; }

        [StringLength(EntityStringFieldLengths.General.Medium)]
        public string EventRaw { get; set; }

        public bool IsMoving { get; set; }

        public decimal? Odometer { get; set; }

        public virtual Truck Truck { get; set; }

        public virtual Driver Driver { get; set; }

    }
}
