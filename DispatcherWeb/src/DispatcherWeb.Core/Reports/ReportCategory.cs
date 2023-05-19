using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispatcherWeb.Reports
{
    [Table("ReportCategory")]
    public partial class ReportCategory : FullAuditedEntity
    {
        public ReportCategory()
        {
            Report = new HashSet<Report>();
        }

        public string Name { get; set; }

        public virtual ICollection<Report> Report { get; set; }
    }
}
