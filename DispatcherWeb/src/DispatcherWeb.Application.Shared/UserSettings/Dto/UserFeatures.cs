﻿namespace DispatcherWeb.UserSettings.Dto
{
    public class UserFeatures
    {
        public bool AllowSharedOrders { get; set; }
        public bool AllowMultiOffice { get; set; }
        public bool AllowSendingOrdersToDifferentTenant { get; set; }
        public bool AllowImportingTruxEarnings { get; set; }

        public bool LeaseHaulers { get; set; }
    }
}