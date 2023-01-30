using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationAppService
    {
        Task<DateTime?> GetScheduledStartTime();
        Task CreateEmployeeTime(CreateEmployeeTimeInput input);
        Task<GetElapsedTimeResult> GetElapsedTime();
        Task SetEmployeeTimeEndDateTime();
        Task<bool> UserIsClockedIn();

        Task<DriverAppInfo> GetDriverAppInfo(GetDriverAppInfoInput input);

        [RemoteService(false)]
        void RemoveOldDriverApplicationLogs();
    }
}