using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Invoices;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Web.Areas.App.Models.Invoices;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Invoices)]
    public class InvoicesController : DispatcherWebControllerBase
    {
        private readonly IInvoiceAppService _invoiceAppService;

        public InvoicesController(IInvoiceAppService invoiceAppService)
        {
            _invoiceAppService = invoiceAppService;
        }

        public IActionResult Index(int? batchId)
        {
            return View(new InvoiceIndexViewModel
            {
                BatchId = batchId
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var model = await _invoiceAppService.GetInvoiceForEdit(new NullableIdDto(id));
            return View(model);
        }

        [Modal]
        public PartialViewResult SelectCustomerTicketsModal(GetCustomerTicketsInput model)
        {
            return PartialView("_SelectCustomerTicketsModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public async Task<FileContentResult> GetInvoicePrintOut(GetInvoicePrintOutInput input)
        {
            var report = await _invoiceAppService.GetInvoicePrintOut(input);
            return InlinePdfFile(report.SaveToBytesArray(), "InvoicePrintOut.pdf");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PartialViewResult> EmailInvoicePrintOutModal(int id)
        {
            var model = await _invoiceAppService.GetEmailInvoicePrintOutModel(new EntityDto(id));
            return PartialView("_EmailInvoicePrintOutModal", model);
        }
    }
}
