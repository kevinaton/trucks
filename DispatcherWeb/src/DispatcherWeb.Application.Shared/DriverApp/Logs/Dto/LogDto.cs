using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApp.Logs.Dto
{
    public class LogDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }

        [StringLength(40)]
        public string AppVersion { get; set; }
        public DateTime DateTime { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }
}
