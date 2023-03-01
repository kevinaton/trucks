using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

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