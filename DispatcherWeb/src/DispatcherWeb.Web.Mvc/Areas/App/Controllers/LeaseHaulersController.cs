using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Drivers.Dto;
using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Web.Areas.App.Models.LeaseHaulers;
using DispatcherWeb.Web.Utils;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
    public class LeaseHaulersController : DispatcherWebControllerBase
    {
        private readonly ILeaseHaulerAppService _leaseHaulerAppService;

        public LeaseHaulersController(ILeaseHaulerAppService leaseHaulerAppService)
        {
            _leaseHaulerAppService = leaseHaulerAppService;
        }

		[AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public IActionResult Index()
        {
            return View();
        }

		[AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerModal(int? id)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditLeaseHaulerModal", model);
        }

		[AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerContactModal(int? id, int? leaseHaulerId)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerContactForEdit(new NullableIdDto(id));

            if (model.LeaseHaulerId == 0 && leaseHaulerId != null)
                model.LeaseHaulerId = leaseHaulerId.Value;

            return PartialView("_CreateOrEditLeaseHaulerContactModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerTruckModal(int? id, int? leaseHaulerId)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerTruckForEdit(new NullableIdDto(id));

            if (model.LeaseHaulerId == 0 && leaseHaulerId != null)
                model.LeaseHaulerId = leaseHaulerId.Value;

            return PartialView("_CreateOrEditLeaseHaulerTruckModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerDriverModal(int? id, int? leaseHaulerId)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerDriverForEdit(new NullableIdDto(id));

            if (model.LeaseHaulerId == 0 && leaseHaulerId != null)
                model.LeaseHaulerId = leaseHaulerId.Value;

            return PartialView("_CreateOrEditLeaseHaulerDriverModal", model);
        }

        public PartialViewResult CallLeaseHaulerContactsModal(int id)
        {
            return PartialView("_CallLeaseHaulerContactsModal", id);
        }

        [Modal]
        public async Task<PartialViewResult> SendMessageModal(int leaseHaulerId, int? leaseHaulerContactId, LeaseHaulerMessageType messageType)
        {
            var model = new SendMessageModalViewModel
            {
                LeaseHaulerId = leaseHaulerId,
                MessageType = messageType,
                Contacts = await _leaseHaulerAppService.GetLeaseHaulerContactSelectList(leaseHaulerId, leaseHaulerContactId, messageType),
            };
            return PartialView("_SendMessageModal", model);
        }


    }
}