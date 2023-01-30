using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Notifications.Dto
{
    public class PriorityNotificationDto
    {
        public Guid Id { get; set; }
        public NotificationSeverity Severity { get; set; }
        public string Message { get; set; }
    }
}
