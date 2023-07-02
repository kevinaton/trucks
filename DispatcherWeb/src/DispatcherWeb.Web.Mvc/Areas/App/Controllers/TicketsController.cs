using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Customers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.Web.Areas.App.Models.Tickets;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View)]
    public class TicketsController : DispatcherWebControllerBase
    {
        private readonly ITicketAppService _ticketService;
        private readonly ICustomerAppService _customerAppService;
        private readonly IUserAppService _userAppService;
        private readonly IRoleAppService _roleAppService;

        public TicketsController(
            ITicketAppService ticketService,
            ICustomerAppService customerAppService,
            IUserAppService userAppService,
            IRoleAppService roleAppService
        )
        {
            _ticketService = ticketService;
            _customerAppService = customerAppService;
            _userAppService = userAppService;
            _roleAppService = roleAppService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userAppService.GetUserForEdit(new NullableIdDto<long>() { Id = AbpSession.UserId.Value });
            var input = new Customers.Dto.GetCustomerContactsInput() { CustomerId = null, Email = user.User.EmailAddress, Sorting = "Name" };
            var customerContacts = await _customerAppService.GetCustomerContacts(input);
            var customerContactWithPortalAccess = customerContacts.Items.FirstOrDefault(p => p.HasCustomerPortalAccess);

            return View(customerContactWithPortalAccess);
        }

        public IActionResult TicketsByDriver()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<PartialViewResult> CreateOrEditTicketModal(int? id, bool? readOnly)
        {
            var model = await _ticketService.GetTicketEditDto(new NullableIdDto(id));
            model.ReadOnly = readOnly;
            return PartialView("_CreateOrEditTicketModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<PartialViewResult> SelectOrderLineModal(LookForExistingOrderLinesInput input)
        {
            var orderLines = await _ticketService.LookForExistingOrderLines(input);
            SelectOrderLineViewModel model =
                new SelectOrderLineViewModel
                {
                    OrderLineSelectList = new SelectList(orderLines, "Id", "Title")
                };
            return PartialView("_SelectOrderLineModal", model);
        }

        [Modal]
        public PartialViewResult SelectDriverModal()
        {
            return PartialView("_SelectDriverModal");
        }

        [Modal]
        public PartialViewResult SelectDateModal()
        {
            return PartialView("_SelectDateModal");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View)]
        public async Task<IActionResult> GetTicketPhoto(int id)
        {
            var ticketPhoto = await _ticketService.GetTicketPhoto(id);
            if (ticketPhoto?.FileBytes == null || string.IsNullOrEmpty(ticketPhoto?.Filename))
            {
                return NotFound();
            }
            if (ticketPhoto.Filename.ToLowerInvariant().EndsWith(".pdf"))
            {
                return InlinePdfFile(ticketPhoto.FileBytes, ticketPhoto.Filename);
            }
            Response.Headers.Add("Content-Disposition", "inline; filename=" + ticketPhoto.Filename.SanitizeFilename());
            return File(ticketPhoto.FileBytes, "image/jpeg");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View)]
        public async Task<IActionResult> GetTicketPhotosForInvoice(int id)
        {
            var ticketPhotos = await _ticketService.GetTicketPhotosForInvoice(id);
            if (ticketPhotos?.FileBytes == null || string.IsNullOrEmpty(ticketPhotos?.Filename))
            {
                return NotFound();
            }
            Response.Headers.Add("Content-Disposition", "inline; filename=" + ticketPhotos.Filename.SanitizeFilename());
            return File(ticketPhotos.FileBytes, "application/zip");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public async Task<FileContentResult> GetTicketPrintOut(GetTicketPrintOutInput input)
        {
            var report = await _ticketService.GetTicketPrintOut(input);
            return InlinePdfFile(report.SaveToBytesArray(), "TicketPrintOut.pdf");
        }
    }
}
