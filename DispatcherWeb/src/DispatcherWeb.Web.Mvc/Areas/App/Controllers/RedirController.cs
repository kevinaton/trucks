using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize]
    public class RedirController : DispatcherWebControllerBase
    {
        public IActionResult Index(string route)
        {
            return Redirect($"/reactapp?route={route}");
        }
    }
}