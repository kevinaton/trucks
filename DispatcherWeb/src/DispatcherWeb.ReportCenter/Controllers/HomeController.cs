using DispatcherWeb.ReportCenter.Models;
using DispatcherWeb.ReportCenter.Services;
using DocumentFormat.OpenXml;
using GrapeCity.Enterprise.Data.Expressions.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.ReportCenter
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly ReportAppService _reportAppService;

        public HomeController(ReportAppService reportAppService)
        {
            _reportAppService = reportAppService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var model = new ReportListViewModel
            {
                Reports = await _reportAppService.GetAvailableReportsList()
            };

            return View(model);
        }

        [Route("/report/{reportPath}")]
        public async Task<IActionResult> Report(string reportPath)
        {
            if (!await _reportAppService.CanAccessReport(reportPath))
                return RedirectToAction("Index");

            var tenantId = 0;
            var claimsDic = HttpContext.User.Claims.ToDictionary(c => c.Type, c => c.Value);
            if (claimsDic.TryGetValue("http://www.aspnetboilerplate.com/identity/claims/tenantId", out string id))
                tenantId = int.Parse(id);

            var vm = new ReportViewModel
            {
                ReportPath = reportPath,
                TenantId = tenantId
            };

            return View(vm);
        }

        [HttpGet("reports")]
        public ActionResult Reports([FromServices] IWebHostEnvironment env)
        {
            string[] validExtensions = { ".rdl", ".rdlx", ".rdlx-master" };

            var reportsPath = $"{env.ContentRootPath}\\Reports\\";
            var reports = new DirectoryInfo(reportsPath).GetFiles()
                    .Where(x => validExtensions.Any(ext => x.Extension.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
                    .Select(x => x.Name)
                    .ToArray();

            return new ObjectResult(reports);
        }

        [HttpGet("report/downloadexportedfile")]
        public ActionResult DownloadExportedFile([FromServices] IWebHostEnvironment env, string fileName, bool delete)
        {
            var exportsPath = $"{env.ContentRootPath}\\Exports\\";

            if (!Directory.Exists(exportsPath))
                Directory.CreateDirectory(exportsPath);

            var targetFilePath = Path.Combine(exportsPath, fileName);

            if (!System.IO.File.Exists(targetFilePath))
                return new NotFoundResult();

            byte[] fileByteArray = System.IO.File.ReadAllBytes(targetFilePath);
            if (delete)
                System.IO.File.Delete(targetFilePath);

            return File(fileByteArray, "text/csv", fileName);
        }

        [Route("/logout")]
        public IActionResult Logout()
        {
            return new SignOutResult(
                new[] {
                     OpenIdConnectDefaults.AuthenticationScheme,
                     CookieAuthenticationDefaults.AuthenticationScheme
                });
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? ControllerContext.HttpContext.TraceIdentifier });
        }

    }
}