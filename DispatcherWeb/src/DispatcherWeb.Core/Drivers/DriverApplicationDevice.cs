using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Drivers
{
    [Table("DriverApplicationDevice")]
    public class DriverApplicationDevice : FullAuditedEntity
    {
        public string Useragent { get; set; }
    }
}
