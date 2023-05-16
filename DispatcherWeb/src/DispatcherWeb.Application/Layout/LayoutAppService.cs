using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Runtime.Session;
using DispatcherWeb.Configuration;
using System.Threading.Tasks;

namespace DispatcherWeb.Layout
{
    [AbpAuthorize]
    public class LayoutAppService : DispatcherWebAppServiceBase, ILayoutAppService
    {
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IAbpSession _abpSession;

        public LayoutAppService(
            IUserNavigationManager userNavigationManager,
            IAbpSession abpSession)
        {
            _userNavigationManager = userNavigationManager;
            _abpSession = abpSession;
        }

        public async Task<UserMenu> GetMenu()
        {
            return await _userNavigationManager.GetMenuAsync("App", _abpSession.ToUserIdentifier());
        }

        public async Task<string> GetSupportLinkAddress()
        {
            return await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.SupportLinkAddress);
        }
    }
}