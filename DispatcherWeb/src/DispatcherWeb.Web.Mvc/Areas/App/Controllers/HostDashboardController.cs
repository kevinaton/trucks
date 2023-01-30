using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Authorization;
using DispatcherWeb.MultiTenancy.HostDashboard;
using DispatcherWeb.Web.Areas.App.Models.HostDashboard;
using DispatcherWeb.Web.Controllers;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
    public class HostDashboardController : DispatcherWebControllerBase
    {
        private const int DashboardOnLoadReportDayCount = 7;

        public ActionResult Index()
        {
            return View(new HostDashboardViewModel(DashboardOnLoadReportDayCount));
        }

    }
}