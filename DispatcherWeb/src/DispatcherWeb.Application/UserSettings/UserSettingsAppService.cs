using Abp.Authorization;
using Abp.Configuration;
using System;

namespace DispatcherWeb.UserSettings
{
    [AbpAuthorize]
    public class UserSettingsAppService : DispatcherWebAppServiceBase, IUserSettingsAppService
    {
        private readonly ISettingManager _settingManager;

        public UserSettingsAppService(
            ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        public bool? GetUserSettingsByName(string settingName)
        {
            var settingValue = _settingManager.GetSettingValue(settingName);
            return !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : null;
        }
    }
}