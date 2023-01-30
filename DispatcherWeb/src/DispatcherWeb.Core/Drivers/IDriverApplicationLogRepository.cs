using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Drivers
{
    public interface IDriverApplicationLogRepository : IRepository<DriverApplicationLog>
    {
        int DeleteLogsEarlierThan(DateTime date);
        int DeleteOldLogs();
    }
}
