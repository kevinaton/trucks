namespace DispatcherWeb.Trucks.Dto
{
    public interface IGetTruckListFilter
    {
        int? OfficeId { get; set; }
        int? VehicleCategoryId { get; set; }
        FilterActiveStatus Status { get; set; }
        bool? IsOutOfService { get; set; }
        bool PlatesExpiringThisMonth { get; set; }
    }
}
