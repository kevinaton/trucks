using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models;
using DispatcherWeb.ReportCenter.Models.ViewModels;
using DispatcherWeb.ReportCenter.Services;
using DocumentFormat.OpenXml;
using GrapeCity.Enterprise.Data.Expressions.Tools;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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

        [Route("/report/{reportPath}/{entityId:int?}")]
        public async Task<IActionResult> Report(string reportPath, int? entityId = null)
        {
            if (!await _reportAppService.CanAccessReport(reportPath))
            {
                throw new UserFriendlyException("You have no access to this report.");
            }

            var tenantId = 0;
            var claimsDic = HttpContext.User.Claims.ToDictionary(c => c.Type, c => c.Value);
            if (claimsDic.TryGetValue("http://www.aspnetboilerplate.com/identity/claims/tenantId", out string id))
                tenantId = int.Parse(id);

            var vm = new ReportViewModel
            {
                ReportPath = reportPath,
                TenantId = tenantId,
                EntityId = entityId
            };

            return View(vm);
        }

        [Route("/report/{reportPath}/{id:int?}/Pdf")]
        public async Task<IActionResult> ReportPdf(string reportPath, int? id = null)
        {
            if (!await _reportAppService.CanAccessReport(reportPath))
                return RedirectToAction("Error");

            var reportId = reportPath.Replace(".rdlx", string.Empty);
            var reportDataDefinition = await _reportAppService.GetReportDataDefinition(reportId);
            await reportDataDefinition.Initialize();

            var memoryPdfStream = reportDataDefinition.OpenReportAsPdf(id);
            var memStream = new MemoryStream();
            memoryPdfStream.WriteTo(memStream);

            await memStream.FlushAsync();
            memStream.Position = 0;

            return new FileStreamResult(memStream, "application/pdf");
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
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? ControllerContext.HttpContext.TraceIdentifier,
                ErrorMessage = exceptionHandlerFeature.Error is UserFriendlyException ? exceptionHandlerFeature.Error.AggregateMessages() : string.Empty
            });
        }
    }
}