using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Projects.Dto
{
    public class ProjectEditDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string Location { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public QuoteStatus Status { get; set; }

        [StringLength(20)]
        public string PONumber { get; set; }

        public string Directions { get; set; }

        public string Notes { get; set; }

        [StringLength(500)]
        public string ChargeTo { get; set; }
    }
}
