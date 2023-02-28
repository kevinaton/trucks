using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.EmployeeTime.Dto
{
    public class EmployeeTimeEditDto
    {
        public int? Id { get; set; }

        public bool LockToCurrentUser { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public decimal? ManualHourAmount { get; set; }

        [Required]
        public int TimeClassificationId { get; set; }

        public string TimeClassificationName { get; set; }

        public bool TimeClassificationAllowsManualTime { get; set; }

        [StringLength(EntityStringFieldLengths.EmployeeTime.Description)]
        public string Description { get; set; }

        public int? TimeOffId { get; set; }
    }
}
