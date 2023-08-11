namespace DispatcherWeb.UserSettings.Dto
{
    public class UserAppSettings
    {
        public bool ValidateUtilization { get; set; }
        public bool AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders { get; set; }
        public bool ShowTrailersOnSchedule { get; set; }
        public bool AllowSubcontractorsToDriveCompanyOwnedTrucks { get; set; }
        public bool AllowSchedulingTrucksWithoutDrivers { get; set; }
    }
}