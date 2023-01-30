using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Events.Bus;
using DispatcherWeb.Infrastructure.BackgroundJobs;

namespace DispatcherWeb.Infrastructure.EventBus.Events
{
    public class ImportCompletedEventData : EventData
    {
        public ImportCompletedEventData(ImportJobArgs args)
        {
            Args = args;
        }
        public ImportJobArgs Args { get; set; }
    }
}
