using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Locations;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Web.Utils;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Locations)]
    public class LocationsController : DispatcherWebControllerBase
    {
        private readonly ILocationAppService _locationAppService;

        public LocationsController(ILocationAppService locationAppService)
        {
            _locationAppService = locationAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditLocationModal(int? id, bool temporary = false, bool mergeWithDuplicateSilently = false)
        {
            var model = await _locationAppService.GetLocationForEdit(new GetLocationForEditInput(id, temporary, mergeWithDuplicateSilently));
            return PartialView("_CreateOrEditLocationModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditSupplierContactModal(int? id, int? locationId)
        {
            var model = await _locationAppService.GetSupplierContactForEdit(new NullableIdDto(id));

            if (model.LocationId == 0 && locationId != null)
                model.LocationId = locationId.Value;

            return PartialView("_CreateOrEditSupplierContactModal", model);
        }
    }
}