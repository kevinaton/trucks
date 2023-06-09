using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.ReportCenter.Models.ViewModels
{
    public class ReportViewModel
    {
        public int TenantId { get; set; }

        public string ReportPath { get; set; }

        public string ReportPathSanitized => string.Join("", ReportPath?.Where(char.IsLetterOrDigit));

        public List<dynamic> CustomDocExports { get; set; }

        public ReportViewModel()
        {
            CustomDocExports = new List<dynamic>();
        }
    }
}
