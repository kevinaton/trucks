using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.ActiveReports.Dto;

namespace DispatcherWeb.ActiveReports
{
    public interface IActiveReportsAppService : IApplicationService
    {
        Task<List<ActiveReportListItemDto>> GetActiveReportsList();
    }
}
