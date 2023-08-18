using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.DriverApp.EmployeeTimes.Dto
{
    public class EmployeeTimeDto
    {
        public int Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        [StringLength(EntityStringFieldLengths.EmployeeTime.Description)]
        public string Description { get; set; }
        public int? TruckId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? TimeClassificationId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public bool IsEditable { get; set; }
        public bool IsImported { get; set; }
    }
}
