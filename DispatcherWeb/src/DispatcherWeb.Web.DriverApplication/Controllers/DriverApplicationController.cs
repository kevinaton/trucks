using DispatcherWeb.Web.DriverApplication.Models.DriverApplication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.DriverApplication.Controllers
{
    public class DriverApplicationController : Controller
    {
        private readonly IConfiguration _configuration;

        public DriverApplicationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("logs")]
        public IActionResult Logs()
        {
            var model = GetIndexViewModel();
            return View(model);
        }

        public IActionResult Index()
        {
            var model = GetIndexViewModel();
            return View(model);
        }

        private IndexViewModel GetIndexViewModel()
        {
            return new IndexViewModel
            {
                IdentityServerUri = _configuration["App:IdentityServerUri"],
                ApiUri = _configuration["App:ApiUri"],
                DriverAppUri = _configuration["App:DriverAppUri"],
                WebPushServerPublicKey = _configuration["WebPush:ServerPublicKey"]
            };
        }
    }
}