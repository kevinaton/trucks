using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleTruckTrailerDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public VehicleCategoryDto VehicleCategory { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public BedConstructionEnum BedConstruction { get; set; }
        public string BedConstructionFormatted => BedConstruction.GetDisplayName();
    }
}
