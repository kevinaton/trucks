using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Public.Controllers
{
    public class HomeController : DispatcherWebControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}