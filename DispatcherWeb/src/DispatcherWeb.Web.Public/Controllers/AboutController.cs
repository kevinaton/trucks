using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Web.Controllers;

namespace DispatcherWeb.Web.Public.Controllers
{
    public class AboutController : DispatcherWebControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}