using Abp.Application.Services;
using DispatcherWeb.Dto;
using DispatcherWeb.Logging.Dto;

namespace DispatcherWeb.Logging
{
    public interface IWebLogAppService : IApplicationService
    {
        GetLatestWebLogsOutput GetLatestWebLogs();

        FileDto DownloadWebLogs();
    }
}
