using Abp.Application.Services;

namespace DispatcherWeb.UserSettings
{
    public interface IUserSettingsAppService : IApplicationService
    {
        string GetUserSettingsByName(string settingName);
    }
}