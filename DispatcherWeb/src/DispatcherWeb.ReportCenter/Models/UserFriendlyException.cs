using System;
using System.Runtime.Serialization;

namespace DispatcherWeb.ReportCenter.Models
{
    [Serializable]
    public class UserFriendlyException : Exception
    {
        public static LogSeverity DefaultLogSeverity = LogSeverity.Warn;

        public LogSeverity Severity { get; set; }

        public UserFriendlyException()
        {
            Severity = DefaultLogSeverity;
        }

        public UserFriendlyException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        public UserFriendlyException(string message)
            : base(message)
        {
            Severity = DefaultLogSeverity;
        }

        public UserFriendlyException(string message, Exception innerException)
            : base(message, innerException)
        {
            Severity = DefaultLogSeverity;
        }

        public enum LogSeverity
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
    }
}
