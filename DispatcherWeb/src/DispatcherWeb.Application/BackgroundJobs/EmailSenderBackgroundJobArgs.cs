using System.Collections.Generic;
using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class EmailSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public List<EmailSenderBackgroundJobArgsEmail> EmailInputs { get; set; }
    }

    public class EmailSenderBackgroundJobArgsEmail
    {
        public string Subject { get; set; }
        public string Text { get; set; }
        public string ToEmailAddress { get; set; }
        public int? DispatchId { get; set; }
        public bool CancelDispatchOnError { get; set; }
        public int? DriverId { get; set; }
    }
}
