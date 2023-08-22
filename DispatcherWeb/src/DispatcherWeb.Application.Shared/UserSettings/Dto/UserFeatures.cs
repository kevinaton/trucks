namespace DispatcherWeb.UserSettings.Dto
{
    public class UserFeatures
    {
        public bool AllowMultiOffice { get; set; }
        public bool AllowSendingOrdersToDifferentTenant { get; set; }
        public bool AllowImportingTruxEarnings { get; set; }
        public bool LeaseHaulers { get; set; }
        public bool DriverProductionPayFeature { get; set; }
    }
}