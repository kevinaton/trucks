using System;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class DispatchingAndMessagingSettingsEditDto
    {
        public DispatchVia DispatchVia { get; set; }
        public bool AllowSmsMessages { get; set; }
        public SendSmsOnDispatchingEnum SendSmsOnDispatching { get; set; }
        public string SmsPhoneNumber { get; set; }
        public string DriverDispatchSms { get; set; }
        public string DriverStartTime { get; set; }
        public bool HideTicketControlsInDriverApp { get; set; }
        public bool RequireDriversToEnterTickets { get; set; }
        public bool RequireSignature { get; set; }
        public bool RequireTicketPhoto { get; set; }
        public string TextForSignatureView { get; set; }
        public bool DispatchesLockedToTruck { get; set; }
        public DateTime DefaultStartTime { get; set; }
        public bool ShowTrailersOnSchedule { get; set; }
        public bool ValidateUtilization { get; set; }
        public bool AllowCounterSales { get; set; }
        public int? DefaultLoadAtLocationId { get; set; }
        public string DefaultLoadAtLocationName { get; set; }
        public bool DefaultDesignationToMaterialOnly { get; set; }
        public bool DefaultAutoGenerateTicketNumber { get; set; }

    }
}
