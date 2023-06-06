using Abp.Application.Services;

namespace DispatcherWeb.UserSettings
{
    public interface IUserSettingsAppService : IApplicationService
    {
        bool? GetUserSettingsByName(string settingName);
    }
}