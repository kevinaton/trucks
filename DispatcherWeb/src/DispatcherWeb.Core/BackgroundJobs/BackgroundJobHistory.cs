using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace DispatcherWeb.BackgroundJobs
{
    [Table("BackgroundJobHistory")]
    public class BackgroundJobHistory : Entity
    {
        public BackgroundJobEnum Job { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Completed { get; set; }
        public string Details { get; set; }
    }
}
