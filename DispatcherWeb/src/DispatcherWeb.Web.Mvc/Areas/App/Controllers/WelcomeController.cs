using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Web.Controllers;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class WelcomeController : DispatcherWebControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}