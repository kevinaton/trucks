using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Drivers
{
    [Table("DriverApplicationDevice")]
    public class DriverApplicationDevice : FullAuditedEntity
    {
        public string Useragent { get; set; }
    }
}
