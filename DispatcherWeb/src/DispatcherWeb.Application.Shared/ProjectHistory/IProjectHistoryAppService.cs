using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.ProjectHistory.Dto;

namespace DispatcherWeb.ProjectHistory
{
    public interface IProjectHistoryAppService : IApplicationService
    {
        Task<PagedResultDto<ProjectHistoryDto>> GetProjectHistory(GetProjectHistoryInput input);
    }
}
