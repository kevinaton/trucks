using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Controllers;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Web.Areas.App.Models.Shared;


namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Reports_RevenueAnalysis)]
    public class RevenueAnalysisReportController : DispatcherWebControllerBase
    {
        public IActionResult Index()
        {
            var model = new ReportViewModelBase
            {
                ShowPdf = false,
            };
            return View(model);            
        }

    }
}