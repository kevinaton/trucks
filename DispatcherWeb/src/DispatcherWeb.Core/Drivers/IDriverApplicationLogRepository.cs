using System;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Drivers
{
    public interface IDriverApplicationLogRepository : IRepository<DriverApplicationLog>
    {
        int DeleteLogsEarlierThan(DateTime date);
        int DeleteOldLogs();
    }
}
