using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class DtdTrackerSettings
    {
        public string AccountName { get; set; }
        public int AccountId { get; internal set; }
        public int UserId { get; internal set; }

        public bool IsEmpty() => string.IsNullOrEmpty(AccountName) || AccountId == 0;
    }
}
