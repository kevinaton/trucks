using System.Collections.Generic;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleTruckDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public int? OfficeId { get; set; }
        public int? SharedWithOfficeId { get; set; }
        public VehicleCategoryDto VehicleCategory { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
        public bool CanPullTrailer { get; set; }
        public bool IsOutOfService { get; set; }
        public bool IsActive { get; set; }
        public decimal Utilization { get; set; }
        public decimal ActualUtilization { get; set; }
        public IList<decimal> UtilizationList { get; set; }
        public bool HasDefaultDriver => DefaultDriverId.HasValue;
        public int? SharedFromOfficeId { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public bool IsExternal { get; set; }
        public int? LeaseHaulerId { get; set; }
        public BedConstructionEnum BedConstruction { get; set; }
        public bool IsApportioned { get; set; }
        public string DefaultDriverName { get; set; }
        public int? DefaultDriverId { get; set; }

        public ScheduleTruckTrailerDto DefaultTrailer { get; set; }
        public ScheduleTruckTractorDto DefaultTractor { get; set; }

        public ScheduleTruckTractorDto Tractor { get; set; }
        public ScheduleTruckTrailerDto Trailer { get; set; }

        public void CopyAllFieldsTo(ScheduleTruckDto other)
        {
            other.CopyAllFieldsFrom(this);
        }

        public void CopyAllFieldsFrom(ScheduleTruckDto other)
        {
            Id = other.Id;
            TruckCode = other.TruckCode;
            OfficeId = other.OfficeId;
            SharedWithOfficeId = other.SharedWithOfficeId;
            SharedFromOfficeId = other.SharedFromOfficeId;
            VehicleCategory = VehicleCategory ?? new VehicleCategoryDto();
            VehicleCategory.Id = other.VehicleCategory.Id;
            VehicleCategory.Name = other.VehicleCategory.Name;
            VehicleCategory.AssetType = other.VehicleCategory.AssetType;
            VehicleCategory.IsPowered = other.VehicleCategory.IsPowered;
            VehicleCategory.SortOrder = other.VehicleCategory.SortOrder;
            AlwaysShowOnSchedule = other.AlwaysShowOnSchedule;
            CanPullTrailer = other.CanPullTrailer;
            IsOutOfService = other.IsOutOfService;
            IsActive = other.IsActive;
            Utilization = other.Utilization;
            ActualUtilization = other.ActualUtilization;
            UtilizationList = other.UtilizationList;
            DriverId = other.DriverId;
            DriverName = other.DriverName;
            IsExternal = other.IsExternal;
            LeaseHaulerId = other.LeaseHaulerId;
            BedConstruction = other.BedConstruction;
            IsApportioned = other.IsApportioned;
            DefaultDriverName = other.DefaultDriverName;
            DefaultDriverId = other.DefaultDriverId;
            DefaultTrailer = other.DefaultTrailer;
            DefaultTractor = other.DefaultTractor;
            Trailer = other.Trailer;
            Tractor = other.Tractor;
        }
    }
}
