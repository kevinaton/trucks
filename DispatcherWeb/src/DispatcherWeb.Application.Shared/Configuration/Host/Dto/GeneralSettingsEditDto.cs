using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Configuration.Host.Dto
{
    public class GeneralSettingsEditDto
    {
        [MaxLength(128)]
        public string WebSiteRootAddress { get; set; }

        public string Timezone { get; set; }

        /// <summary>
        /// This value is only used for comparing user's timezone to default timezone
        /// </summary>
        public string TimezoneForComparison { get; set; }

        /// <summary>
        /// This value maps windows timezone to IANA time zone
        /// </summary>
        public string TimezoneIana { get; set; }

        public string OrderEmailBodyTemplate { get; set; }

        public string OrderEmailSubjectTemplate { get; set; }

        public string ReceiptEmailBodyTemplate { get; set; }

        public string ReceiptEmailSubjectTemplate { get; set; }

        public string DriverOrderEmailTitle { get; set; }
        public string DriverOrderEmailBody { get; set; }
        public string DriverOrderSms { get; set; }

        public string CompanyName { get; set; }
        public string DefaultMapLocation { get; set; }
        public string DefaultMapLocationAddress { get; set; }
        public string CurrencySymbol { get; set; }
        public string UserDefinedField1 { get; set; }
        public bool DontValidateDriverAndTruckOnTickets { get; set; }
        public bool ShowDriverNamesOnPrintedOrder { get; set; }
        public bool SplitBillingByOffices { get; set; }
        public bool UseShifts { get; set; }
        public string ShiftName1 { get; set; }
        public string ShiftName2 { get; set; }
        public string ShiftName3 { get; set; }
        public string NotificationsEmail { get; set; }
        public string SupportLinkAddress { get; set; }
        public DriverAppImageResolutionEnum DriverAppImageResolution { get; set; }
    }
}