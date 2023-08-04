using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.UserSettings.Dto;

namespace DispatcherWeb.UserSettings
{
    public interface IUserSettingsAppService : IApplicationService
    {
        Task<UserConfig> GetUserAppConfig();
        Task<GeneralSettingsEditDto> GetGeneralSettings();
        Task<string> GetUserSettingByName(string settingName);
    }
}