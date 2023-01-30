using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UploadLogsInput
    {
        public long Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public bool? Sw { get; set; }
    }
}
