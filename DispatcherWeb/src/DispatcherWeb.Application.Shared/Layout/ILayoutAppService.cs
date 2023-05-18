using Abp.Application.Navigation;
using Abp.Application.Services;
using System.Threading.Tasks;

namespace DispatcherWeb.Layout
{
    public interface ILayoutAppService : IApplicationService
    {
        Task<UserMenu> GetMenu();

        Task<string> GetSupportLinkAddress();
    }
}