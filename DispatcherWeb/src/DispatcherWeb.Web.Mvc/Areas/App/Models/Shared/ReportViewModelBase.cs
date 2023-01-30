using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.Areas.App.Models.Shared
{
    public class ReportViewModelBase
    {
        public bool ShowPdf { get; set; } = true;
        public bool ShowCsv { get; set; } = true;
    }
}
