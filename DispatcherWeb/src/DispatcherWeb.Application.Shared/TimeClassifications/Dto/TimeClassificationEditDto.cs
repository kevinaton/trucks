using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DispatcherWeb.TimeClassifications.Dto
{
    public class TimeClassificationEditDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public bool IsProductionBased { get; set; }

        public decimal? DefaultRate { get; set; }

        public bool HasRecordsAssociated { get; set; }
    }
}
