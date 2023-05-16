using Abp.Authorization;
using DispatcherWeb.Configuration;
using System.Threading.Tasks;

namespace DispatcherWeb.Layout
{
    [AbpAuthorize]
    public class LayoutAppService : DispatcherWebAppServiceBase, ILayoutAppService
    {
        public async Task<string> GetSupportLinkAddress()
        {
            return await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.SupportLinkAddress);
        }
    }
}