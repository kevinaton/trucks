using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.EmployeeTime;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.EmployeeTime.Dto;
using System;
using Abp.Timing;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_TimeEntry)]
    public class EmployeeTimeController : DispatcherWebControllerBase
    {
        private readonly IEmployeeTimeAppService _employeeTimeAppService;

        public EmployeeTimeController(IEmployeeTimeAppService employeeTimeAppService)
        {
            _employeeTimeAppService = employeeTimeAppService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _employeeTimeAppService.GetEmployeeTimeIndexModel();
            return View(model);
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _employeeTimeAppService.GetEmployeeTimeForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }

        public async Task<PartialViewResult> AddBulkTimeModal()
        {
            var tz = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            var model = new AddBulkTimeDto()
            {
                StartDateTime = DateTime.UtcNow.ConvertTimeZoneTo(tz),
                EndDateTime = DateTime.UtcNow.ConvertTimeZoneTo(tz)
            };

            return PartialView("_AddBulkTimeModal", model);
        }
    }
}