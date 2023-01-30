using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverApp.Settings.Dto
{
    public class SettingsDto
    {
        public int HttpRequestTimeout { get; set; }
        public bool HideTicketControls { get; set; }
        public bool RequireToEnterTickets { get; set; }
        public bool RequireSignature { get; set; }
        public bool RequireTicketPhoto { get; set; }
        public string TextForSignatureView { get; set; }
        public bool IsUserAdmin { get; set; }
        public bool IsUserDriver { get; set; }
        public bool IsUserLeaseHaulerDriver { get; set; }
    }
}
