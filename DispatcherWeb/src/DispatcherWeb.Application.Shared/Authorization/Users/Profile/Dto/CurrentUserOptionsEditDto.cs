namespace DispatcherWeb.Authorization.Users.Profile.Dto
{
    public class CurrentUserOptionsEditDto
    {
        public bool DontShowZeroQuantityWarning { get; set; }
        public bool PlaySoundForNotifications { get; set; }
        public bool DefaultDesignationToCounterSales { get; set; }
        public int? DefaultLoadAtLocationId { get; set; }
        public string DefaultLoadAtLocationName { get; set; }
        public int? DefaultServiceId { get; set; }
        public string DefaultServiceName { get; set; }
        public int? DefaultMaterialUomId { get; set; }
        public string DefaultMaterialUomName { get; set; }
        public bool DefaultAutoGenerateTicketNumber { get; set; }
        public bool CCMeOnInvoices { get; set; }
    }
}
