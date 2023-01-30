using System;
using System.Collections.Generic;
using System.Text;
using Abp.Auditing;
using Abp.EntityFrameworkCore;
using Castle.Core.Logging;
using DispatcherWeb.Auditing;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class AuditLogRepository : DispatcherWebRepositoryBase<AuditLog, long>, IAuditLogRepository
    {
        public AuditLogRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider, ILogger logger) 
            : base(dbContextProvider)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        public int DeleteOldAuditLogs()
        {
            Logger.Info($"AuditLogRepository.DeleteOldAuditLogs started");

            var rowsAffected = GetContext().Database.ExecuteSqlRaw(
                "EXEC RemoveOldAuditLogs"
            );

            Logger.Info($"AuditLogRepository.DeleteOldAuditLogs finished: {rowsAffected} rows affected");

            return rowsAffected;
        }
    }
}
