using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Attributes;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.VehicleMaintenance;

namespace DispatcherWeb.Trucks
{
    [Table("Truck")]
    public class Truck : FullAuditedEntity, IMustHaveTenant
    {
        public const int MaxTruckCodeLength = 25;

        public Truck()
        {
            OrderLineTrucks = new HashSet<OrderLineTruck>();
            DriverAssignments = new HashSet<DriverAssignment>();
            SharedTrucks = new HashSet<SharedTruck>();
            Files = new HashSet<TruckFile>();
            OutOfServiceHistories = new HashSet<OutOfServiceHistory>();
            Tickets = new HashSet<Ticket>();
            AvailableLeaseHaulerTrucks = new HashSet<AvailableLeaseHaulerTruck>();
        }

        public int TenantId { get; set; }

        [Required(ErrorMessage = "Truck Code is a required field")]
        [StringLength(MaxTruckCodeLength)]
        public string TruckCode { get; set; }

        public int? LocationId { get; set; }

        [Obsolete]
        [Range(1, 5, ErrorMessage = "Category is a required field")]
        public TruckCategory Category { get; set; }

        public int VehicleCategoryId { get; set; }

        public virtual VehicleCategory VehicleCategory { get; set; }

        public bool IsActive { get; set; }

        public DateTime? InactivationDate { get; set; }

        public bool IsOutOfService { get; set; }

        public bool IsApportioned { get; set; }

        [Obsolete("Use LeaseHaulerTruck.AlwaysShowOnSchedule instead")]
        public bool IsEmbedded { get; set; }

        public bool CanPullTrailer { get; set; }

        public BedConstructionEnum BedConstruction { get; set; }

        [ForeignKey("LocationId")]
        public virtual Office Office { get; set; }

        public int? DefaultDriverId { get; set; }

        public int? DefaultTrailerId { get; set; }
        public Truck DefaultTrailer { get; set; }

        public virtual Driver DefaultDriver { get; set; }

        public virtual ICollection<OrderLineTruck> OrderLineTrucks { get; set; }

        public virtual ICollection<DriverAssignment> DriverAssignments { get; set; }

        public virtual ICollection<SharedTruck> SharedTrucks { get; set; }

        public virtual ICollection<AvailableLeaseHaulerTruck> AvailableLeaseHaulerTrucks { get;set; }

        [MileageColumn]
        public decimal CurrentMileage { get; set; }

        public decimal CurrentHours { get; set; }

        public ICollection<PreventiveMaintenance> PreventiveMaintenances { get; set; }

        public ICollection<WorkOrder> WorkOrders { get; set; }


        public int? Year { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Make)]
        public string Make { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Model)]
        public string Model { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Vin)]
        public string Vin { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Plate)]
        public string Plate { get; set; }

        public DateTime? PlateExpiration { get; set; }

        public decimal? CargoCapacity { get; set; }

        public decimal? CargoCapacityCyds { get; set; }

        public FuelType? FuelType { get; set; }

        public int? FuelCapacity { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.SteerTires)]
        public string SteerTires { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DriveAxleTires)]
        public string DriveAxleTires { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DropAxleTires)]
        public string DropAxleTires { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.TrailerTires)]
        public string TrailerTires { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Transmission)]
        public string Transmission { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Engine)]
        public string Engine { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.RearEnd)]
        public string RearEnd { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.InsurancePolicyNumber)]
        public string InsurancePolicyNumber { get; set; }

        public DateTime? InsuranceValidUntil { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [Column(TypeName = DispatcherWebConsts.DbTypeDecimal19_4)]
        public decimal? PurchasePrice { get; set; }

        [Required]
        public DateTime InServiceDate { get; set; }

        public DateTime? SoldDate { get; set; }

        [Column(TypeName = DispatcherWebConsts.DbTypeDecimal19_4)]
        public decimal? SoldPrice { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.TruxTruckId)]
        public string TruxTruckId { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DtdTrackerUniqueId)]
        public string DtdTrackerUniqueId { get; set; }

        public long? DtdTrackerDeviceTypeId { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DtdTrackerDeviceTypeName)]
        public string DtdTrackerDeviceTypeName { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DtdTrackerServerAddress)]
        public string DtdTrackerServerAddress { get; set; }

        //[StringLength(EntityStringFieldLengths.Truck.DtdTrackerPassword)]
        public string DtdTrackerPassword { get; set; }

        //we can add this later if it is needed
        //public bool HasMaterialCompanyTrucks { get; set; }

        /// <summary>
        /// HaulingCompany's order line id. Only set for MaterialCompany trucks when a copy of this truck exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTruckId { get; set; }
        /// <summary>
        /// HaulingCompany's tenant id. Only set for MaterialCompany trucks when a copy of this truck exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        public ICollection<TruckFile> Files { get; set; }

        public ICollection<OutOfServiceHistory> OutOfServiceHistories { get; set; }

        public ICollection<Ticket> Tickets { get; set; }

        public ICollection<VehicleUsage> VehicleUsages { get; set; }

        //public ICollection<LeaseHaulerTruck> LeaseHaulerTrucks { get; set; }
        public LeaseHaulerTruck LeaseHaulerTruck { get; set; }
    }
}
