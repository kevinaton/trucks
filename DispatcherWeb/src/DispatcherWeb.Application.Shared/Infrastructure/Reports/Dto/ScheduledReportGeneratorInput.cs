using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Reports.Dto
{
	[Serializable]
    public class ScheduledReportGeneratorInput
    {
		public ReportType ReportType { get; set; }
		public ReportFormat ReportFormat { get; set; }
		public string[] EmailAddresses { get; set; }

		public CustomSession CustomSession { get; set; }
    }
}
