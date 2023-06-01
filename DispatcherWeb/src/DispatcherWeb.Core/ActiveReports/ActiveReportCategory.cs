using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispatcherWeb.Reports
{
    [Table("ActiveReportCategory")]
    public partial class ActiveReportCategory : FullAuditedEntity
    {
        public ActiveReportCategory()
        {
            Report = new HashSet<ActiveReport>();
        }

        public string Name { get; set; }

        public virtual ICollection<ActiveReport> Report { get; set; }
    }
}
