using System.Threading.Tasks;
using Abp.Application.Services;

namespace DispatcherWeb.UserSettings
{
    public interface IUserSettingsAppService : IApplicationService
    {
        Task<string> GetUserSettingsByName(string settingName);
    }
}