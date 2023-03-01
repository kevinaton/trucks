using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

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