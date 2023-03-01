using System.Collections.Generic;
using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class SmsSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public List<SmsSenderBackgroundJobArgsSms> SmsInputs { get; set; }
    }

    public class SmsSenderBackgroundJobArgsSms
    {
        public string Text { get; set; }
        public string ToPhoneNumber { get; set; }
        public bool TrackStatus { get; set; } = false;
        public bool UseTenantPhoneNumberOnly { get; set; } = false;
        public int? DispatchId { get; set; }
        public bool CancelDispatchOnError { get; set; }
        public int? DriverId { get; set; }
    }
}
