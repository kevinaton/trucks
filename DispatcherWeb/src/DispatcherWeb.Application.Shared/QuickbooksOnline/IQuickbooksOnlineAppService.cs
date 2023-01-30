using Abp.Application.Services;
using DispatcherWeb.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.QuickbooksOnline
{
    public interface IQuickbooksOnlineAppService : IApplicationService
    {
        Task HandleAuthCallback(string code, string realmId, string state);
        Task<List<SelectListDto>> GetIncomeAccountSelectList();
    }
}
