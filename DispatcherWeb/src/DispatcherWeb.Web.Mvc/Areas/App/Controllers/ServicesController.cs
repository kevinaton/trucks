using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Services;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Services)]
    public class ServicesController : DispatcherWebControllerBase
    {
        private readonly IServiceAppService _serviceAppService;

        public ServicesController(IServiceAppService serviceAppService)
        {
            _serviceAppService = serviceAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditServiceModal(NullableIdNameDto input)
        {
            var model = await _serviceAppService.GetServiceForEdit(input);
            if (!model.Id.HasValue && !string.IsNullOrEmpty(input.Name))
            {
                model.Service1 = input.Name;
            }
            return PartialView("_CreateOrEditServiceModal", model);
        }

        public async Task<PartialViewResult> CreateOrEditServicePriceModal(int? id, int? serviceId)
        {
            var model = await _serviceAppService.GetServicePriceForEdit(new NullableIdDto(id));

            if (model.ServiceId == 0 && serviceId != null)
                model.ServiceId = serviceId.Value;

            return PartialView("_CreateOrEditServicePriceModal", model);
        }
    }
}