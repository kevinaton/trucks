using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispatcherWeb.Reports
{
    [Table("Report")]
    public partial class Report : FullAuditedEntity
    {
        public Report()
        {

        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }

        public int CategoryId { get; set; }
        public virtual ReportCategory Category { get; set; }
    }
}
