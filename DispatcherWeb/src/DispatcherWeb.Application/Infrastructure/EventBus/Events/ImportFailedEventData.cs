using Abp.Events.Bus;
using DispatcherWeb.Infrastructure.BackgroundJobs;

namespace DispatcherWeb.Infrastructure.EventBus.Events
{
    public class ImportFailedEventData : EventData
    {
        public ImportFailedEventData(ImportJobArgs args)
        {
            Args = args;
        }
        public ImportJobArgs Args { get; set; }
    }
}
