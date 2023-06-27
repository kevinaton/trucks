namespace DispatcherWeb.Scheduling.Dto
{
    public class TruckToAssignDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public int? LeaseHaulerId { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public bool IsApportioned { get; set; }
        public BedConstructionEnum BedConstruction { get; set; }
        public string BedConstructionName => BedConstruction.ToString();
    }
}
