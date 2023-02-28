﻿using Abp.Auditing;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Auditing
{
    public interface IAuditLogRepository : IRepository<AuditLog, long>
    {
        int DeleteOldAuditLogs();
    }
}
