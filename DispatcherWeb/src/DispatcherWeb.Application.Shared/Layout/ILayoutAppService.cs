using Abp.Application.Services;
using System.Threading.Tasks;

namespace DispatcherWeb.Layout
{
    public interface ILayoutAppService : IApplicationService
    {
        Task<string> GetSupportLinkAddress();
    }
}