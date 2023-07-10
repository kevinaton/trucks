using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Configuration.Host.Dto;

namespace DispatcherWeb.UserSettings
{
    public interface IUserSettingsAppService : IApplicationService
    {
        Task<GeneralSettingsEditDto> GetGeneralSettings();
        Task<string> GetUserSettingByName(string settingName);
    }
}