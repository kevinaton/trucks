using Abp.Authorization;
using Abp.Configuration;
using System;

namespace DispatcherWeb.UserSettings
{
    [AbpAuthorize]
    public class UserSettingsAppService : DispatcherWebAppServiceBase, IUserSettingsAppService
    {
        public string GetUserSettingsByName(string settingName)
        {
            return SettingManager.GetSettingValue(settingName);
        }
    }
}