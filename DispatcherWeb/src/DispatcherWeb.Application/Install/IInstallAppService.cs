using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Install.Dto;

namespace DispatcherWeb.Install
{
    public interface IInstallAppService : IApplicationService
    {
        Task Setup(InstallDto input);

        AppSettingsJsonDto GetAppSettingsJson();

        CheckDatabaseOutput CheckDatabase();
    }
}