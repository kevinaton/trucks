using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;

namespace DispatcherWeb.UserSettings
{
    [AbpAuthorize]
    public class UserSettingsAppService : DispatcherWebAppServiceBase, IUserSettingsAppService
    {
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        private readonly IScopedIocResolver _scopedIocResolver;

        public UserSettingsAppService(
            ISettingDefinitionManager settingDefinitionManager,
            IScopedIocResolver scopedIocResolver
            )
        {
            _settingDefinitionManager = settingDefinitionManager;
            _scopedIocResolver = scopedIocResolver;
        }

        public async Task<string> GetUserSettingsByName(string settingName)
        {
            var settingDefinition = _settingDefinitionManager.GetSettingDefinition(settingName);
            if (settingDefinition.ClientVisibilityProvider == null 
                || !await settingDefinition.ClientVisibilityProvider.CheckVisible(_scopedIocResolver))
            {
                throw new ApplicationException($"Setting {settingName} is not set to be client-side visible");
            }

            return SettingManager.GetSettingValue(settingName);
        }
    }
}