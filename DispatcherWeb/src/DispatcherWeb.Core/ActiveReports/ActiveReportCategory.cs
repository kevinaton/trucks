using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispatcherWeb.ActiveReports
{
    [Table("ActiveReportCategory")]
    public partial class ActiveReportCategory : FullAuditedEntity
    {
        public ActiveReportCategory()
        {
            Reports = new HashSet<ActiveReport>();
        }

        public string Name { get; set; }

        public virtual ICollection<ActiveReport> Reports { get; set; }
    }
}
