using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.CspReports.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.CspReports
{
    public class CspReportAppService : DispatcherWebAppServiceBase, ICspReportAppService
    {
        public void PostReport(PostReportDto postReport)
        {
           // Nothing here. Look Audit Logs for data
        }
    }
}
