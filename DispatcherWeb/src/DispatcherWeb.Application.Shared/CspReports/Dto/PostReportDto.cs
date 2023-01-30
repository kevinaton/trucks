using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DispatcherWeb.CspReports.Dto
{
    public class PostReportDto
    {
        [JsonProperty(PropertyName = "csp-report")]
        public CspReportDto CspReport { get; set; }
    }
}
