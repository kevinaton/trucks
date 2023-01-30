using Microsoft.AspNetCore.Antiforgery;

namespace DispatcherWeb.Web.Controllers
{
    public class AntiForgeryController : DispatcherWebControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
