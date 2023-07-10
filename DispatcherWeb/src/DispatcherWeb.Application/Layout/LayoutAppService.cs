using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.Runtime.Session;
using DispatcherWeb.Configuration;
using DispatcherWeb.Layout.Dto;
using DispatcherWeb.Sessions;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Layout
{
    [AbpAuthorize]
    public class LayoutAppService : DispatcherWebAppServiceBase, ILayoutAppService
    {
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly ILanguageManager _languageManager;
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly ISessionAppService _sessionAppService;
        private readonly IAbpSession _abpSession;

        public LayoutAppService(
            IMultiTenancyConfig multiTenancyConfig,
            ILanguageManager languageManager,
            IUserNavigationManager userNavigationManager,
            ISessionAppService sessionAppService,
            IAbpSession abpSession)
        {
            _multiTenancyConfig = multiTenancyConfig;
            _languageManager = languageManager;
            _userNavigationManager = userNavigationManager;
            _sessionAppService = sessionAppService;
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

        public async Task<UserProfileDto> GetUserProfile()
        {
            var data = new UserProfileDto
            {
                LoginInformations = await _sessionAppService.GetCurrentLoginInformations(),
                Languages = _languageManager.GetLanguages().Where(l => !l.IsDisabled).ToList(),
                CurrentLanguage = _languageManager.CurrentLanguage,
                IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
                IsImpersonatedLogin = AbpSession.ImpersonatorUserId.HasValue,
                SubscriptionExpireNootifyDayCount = SettingManager.GetSettingValue<int>(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount),
                SessionOfficeId = Session.OfficeId,
                SessionOfficeName = Session.OfficeName
            };

            return data;
        }
    }
}