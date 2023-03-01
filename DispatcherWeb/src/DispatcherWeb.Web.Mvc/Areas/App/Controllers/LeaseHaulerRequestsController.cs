using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerRequests)]
    public class LeaseHaulerRequestsController : DispatcherWebControllerBase
    {
        private readonly ILeaseHaulerRequestEditAppService _leaseHaulerRequestEdit;

        public LeaseHaulerRequestsController(
            ILeaseHaulerRequestEditAppService leaseHaulerRequestEdit
            )
        {
            _leaseHaulerRequestEdit = leaseHaulerRequestEdit;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerRequestModal(int? leaseHaulerRequestId)
        {
            var model = await _leaseHaulerRequestEdit.GetLeaseHaulerRequestEditDto(leaseHaulerRequestId);
            return PartialView("_CreateOrEditLeaseHaulerRequestModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
        public PartialViewResult SendLeaseHaulerRequestModal()
        {
            var model = new LeaseHaulerRequestEditDto();
            return PartialView("_SendLeaseHaulerRequestModal", model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> AvailableTrucks(string id)
        {
            if (!ShortGuid.TryParse(id, out var guid))
            {
                return View("../../../../Views/Error/Error404");
            };

            var model = await _leaseHaulerRequestEdit.GetAvailableTrucksEditDto(guid);

            if (model == null)
            {
                return View("../../../../Views/Error/Error404");
            }

            if (model.IsExpired)
            {
                return View("AvailableTrucksLinkExpired");
            }

            return View(model);
        }
    }
}
