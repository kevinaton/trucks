using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Projects.Dto;

namespace DispatcherWeb.Projects
{
    public interface IProjectAppService : IApplicationService
    {
        Task<PagedResultDto<ProjectDto>> GetProjects(GetProjectsInput input);
        Task<PagedResultDto<SelectListDto>> GetProjectsSelectList(GetSelectListInput input);
        Task<ProjectEditDto> GetProjectForEdit(NullableIdNameDto input);
        Task<ProjectEditDto> EditProject(ProjectEditDto model);
        Task<bool> CanDeleteProject(EntityDto input);
        Task DeleteProject(EntityDto input);
        Task InactivateProject(EntityDto input);

        Task<PagedResultDto<ProjectServiceDto>> GetProjectServices(GetProjectServicesInput input);
        Task<ProjectServiceEditDto> GetProjectServiceForEdit(GetProjectServiceForEditInput input);
        Task EditProjectService(ProjectServiceEditDto model);
        Task DeleteProjectService(EntityDto input);

    }
}
