using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace DispatcherWeb.Web.Controllers
{
    public class ReactAppController : DispatcherWebControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ReactAppController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "ClientApp/build", "index.html");
            return PhysicalFile(filePath, "text/html");
        }
    }
}