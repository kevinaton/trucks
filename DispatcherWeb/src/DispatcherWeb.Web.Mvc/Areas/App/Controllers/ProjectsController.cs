using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Projects;
using DispatcherWeb.Projects.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Projects)]
    public class ProjectsController : DispatcherWebControllerBase
    {
        private readonly IProjectAppService _projectAppService;

        public ProjectsController(IProjectAppService projectAppService)
        {
            _projectAppService = projectAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(NullableIdNameDto input)
        {
            var model = await _projectAppService.GetProjectForEdit(input);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Details(ProjectEditDto model)
        {
            var id = await _projectAppService.EditProject(model);
            return RedirectToAction("Details", new { id });
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditProjectModal(NullableIdNameDto input)
        {
            var model = await _projectAppService.GetProjectForEdit(input);
            return PartialView("_CreateOrEditProjectModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditProjectServiceModal(int? id, int? projectId)
        {
            var model = await _projectAppService.GetProjectServiceForEdit(new GetProjectServiceForEditInput(id, projectId));
            return PartialView("_CreateOrEditProjectServiceModal", model);
        }
    }
}
