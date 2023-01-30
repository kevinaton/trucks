using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.Projects
{
    [Table("Project")]
    public class Project : FullAuditedEntity, IMustHaveTenant
    {
        public const int MaxNameLength = 50;

        public Project()
        {
            ProjectServices = new HashSet<ProjectService>();
            Quotes = new HashSet<Quote>();
        }

        public int TenantId { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(20)]
        public string PONumber { get; set; }

        [StringLength(500)]
        public string Location { get; set; }

        [StringLength(500)]
        public string ChargeTo { get; set; }

        public string Directions { get; set; }

        public string Notes { get; set; }

        public ProjectStatus Status { get; set; }

        public virtual ICollection<ProjectService> ProjectServices { get; set; }

        public virtual ICollection<ProjectHistoryRecord> ProjectHistory { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; }
    }
}
