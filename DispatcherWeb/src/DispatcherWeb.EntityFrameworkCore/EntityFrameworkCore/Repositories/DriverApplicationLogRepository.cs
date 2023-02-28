using System;
using Abp.EntityFrameworkCore;
using Castle.Core.Logging;
using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class DriverApplicationLogRepository : DispatcherWebRepositoryBase<DriverApplicationLog>, IDriverApplicationLogRepository
    {
        public DriverApplicationLogRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider, ILogger logger)
            : base(dbContextProvider)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        public int DeleteLogsEarlierThan(DateTime date)
        {
            var context = GetContext();
            var rowsAffected = context.Database.ExecuteSqlInterpolated(
                $"delete from DriverApplicationLog where DateTime < {date}"
            );

            Logger.Info($"DriverApplicationLogRepository.DeleteLogsEarlierThan {date:s}: {rowsAffected} rows affected");

            return rowsAffected;
        }

        public int DeleteOldLogs()
        {
            Logger.Info($"DriverApplicationLogRepository.DeleteOldLogs started");

            var context = GetContext();
            var rowsAffected = context.Database.ExecuteSqlRaw(
                "EXEC RemoveOldDriverAppLogs"
            );

            Logger.Info($"DriverApplicationLogRepository.DeleteOldLogs finished: {rowsAffected} rows affected");

            return rowsAffected;
        }
    }
}
