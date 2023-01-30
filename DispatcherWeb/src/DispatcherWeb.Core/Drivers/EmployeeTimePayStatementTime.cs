using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.PayStatements;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Drivers
{
    [Table("EmployeeTimePayStatementTime")]
    public class EmployeeTimePayStatementTime : CreationAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public int EmployeeTimeId { get; set; }
        public virtual EmployeeTime EmployeeTime { get; set; }
        public int PayStatementTimeId { get; set; }
        public virtual PayStatementTime PayStatementTime { get; set; }
    }
}
