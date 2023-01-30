using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Customers;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Customers)]
    public class CustomersController : DispatcherWebControllerBase
    {
        private readonly ICustomerAppService _customerAppService;

        public CustomersController(ICustomerAppService customerAppService)
        {
            _customerAppService = customerAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditCustomerModal(int? id)
        {
            var model = await _customerAppService.GetCustomerForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditCustomerModal", model);
        }

        public async Task<PartialViewResult> CreateOrEditCustomerContactModal(int? id, int? customerId)
        {
            var model = await _customerAppService.GetCustomerContactForEdit(new NullableIdDto(id));

            if (model.CustomerId == 0 && customerId != null)
                model.CustomerId = customerId.Value;

            return PartialView("_CreateOrEditCustomerContactModal", model);
        }
    }
}