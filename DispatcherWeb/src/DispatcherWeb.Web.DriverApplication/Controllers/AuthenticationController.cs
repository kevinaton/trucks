using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.DriverApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Callback()
        {
            return View();
        }

        public IActionResult SilentCallback()
        {
            return View();
        }

        public IActionResult SignoutCallback()
        {
            return View();
        }

        public IActionResult LoggedOut()
        {
            return View();
        }
    }
}