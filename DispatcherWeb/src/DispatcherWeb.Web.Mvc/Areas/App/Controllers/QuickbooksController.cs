using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.UI;
using DispatcherWeb.QuickbooksOnline;
using DispatcherWeb.QuickbooksDesktop;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class QuickbooksController : DispatcherWebControllerBase
    {
        private readonly IQuickbooksOnlineAppService _quickbooksOnlineAppService;
        private readonly IQuickbooksDesktopAppService _quickbooksDesktopAppService;

        public QuickbooksController(
            IQuickbooksOnlineAppService quickbooksOnlineAppService,
            IQuickbooksDesktopAppService quickbooksDesktopAppService
            )
        {
            _quickbooksOnlineAppService = quickbooksOnlineAppService;
            _quickbooksDesktopAppService = quickbooksDesktopAppService;
        }

        public async Task<ActionResult> Callback(string code = "none", string realmId = "none", string state = null)
        {
            try
            {
                await _quickbooksOnlineAppService.HandleAuthCallback(code, realmId, state);
            }
            catch (UserFriendlyException ex)
            {
                return View("Error", ex.Message + "; " + ex.Details);
            }
            return RedirectToAction("QuickbooksOnline", "Settings", new { area = "App" });
        }

        public async Task<IActionResult> ExportInvoicesToIIF()
        {
            var result = await _quickbooksDesktopAppService.ExportInvoicesToIIF();
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return View("Error", result.ErrorMessage);
            }
            return File(result.FileBytes, "text/plain", $"InvoicesBatch-{result.UploadBatchId}.iif");
        }
    }
}