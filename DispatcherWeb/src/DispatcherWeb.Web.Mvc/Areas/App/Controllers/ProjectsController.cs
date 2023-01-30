using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Projects;
using DispatcherWeb.Projects.Dto;
using DispatcherWeb.Web.Controllers;
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
        public async Task<IActionResult> Details(int? id)
        {
            var model = await _projectAppService.GetProjectForEdit(new NullableIdDto(id));
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Details(ProjectEditDto model)
        {
            var id = await _projectAppService.EditProject(model);
            return RedirectToAction("Details", new { id });
        }
        
        public async Task<PartialViewResult> CreateOrEditProjectServiceModal(int? id, int? projectId)
        {
            var model = await _projectAppService.GetProjectServiceForEdit(new GetProjectServiceForEditInput(id, projectId));
            return PartialView("_CreateOrEditProjectServiceModal", model);
        }
    }
}
