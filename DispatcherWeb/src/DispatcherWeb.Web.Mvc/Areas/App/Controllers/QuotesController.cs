using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Quotes;
using DispatcherWeb.Quotes.Dto;
using DispatcherWeb.Web.Areas.App.Models.Quotes;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Quotes_View)]
    public class QuotesController : DispatcherWebControllerBase
    {
        private readonly IQuoteAppService _quoteAppService;

        public QuotesController(IQuoteAppService quoteAppService)
        {
            _quoteAppService = quoteAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id, int? projectId)
        {
            var model = await _quoteAppService.GetQuoteForEdit(new GetQuoteForEditInput(id, projectId));
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Details(QuoteEditDto model)
        {
            var id = await _quoteAppService.EditQuote(model);
            return RedirectToAction("Details", new { id });
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditQuoteServiceModal(int? id, int? quoteId)
        {
            var model = await _quoteAppService.GetQuoteServiceForEdit(new GetQuoteServiceForEditInput(id, quoteId));
            return PartialView("_CreateOrEditQuoteServiceModal", model);
        }

        [Modal]
        public PartialViewResult ViewQuoteDeliveriesModal(ViewQuoteDeliveriesViewModel model)
        {
            return PartialView("_ViewQuoteDeliveriesModal", model);
        }

        [Modal]
        public PartialViewResult AddToProjectModal()
        {
            return PartialView("_AddToProjectModal");
        }

        public async Task<IActionResult> Copy(int id)
        {
            var newId = await _quoteAppService.CopyQuote(new EntityDto(id));
            return RedirectToAction("Details", new { id = newId });
        }

        public async Task<IActionResult> GetReport(GetQuoteReportInput input)
        {
            var report = await _quoteAppService.GetQuoteReport(input);
            return InlinePdfFile(report, "QuoteReport.pdf");
        }

        public async Task<PartialViewResult> EmailQuoteReportModal(int id)
        {
            var model = await _quoteAppService.GetEmailQuoteReportModel(new EntityDto(id));
            return PartialView("_EmailQuoteReportModal", model);
        }
    }
}
