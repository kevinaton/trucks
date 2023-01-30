using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Notifications;

namespace DispatcherWeb.Infrastructure.Notifications
{
    public class ImportCompletedNotificationData : NotificationData
    {
        public ImportCompletedNotificationData(string url)
        {
            Url = url;
        }

        public string Url { get; set; }
    }
}
