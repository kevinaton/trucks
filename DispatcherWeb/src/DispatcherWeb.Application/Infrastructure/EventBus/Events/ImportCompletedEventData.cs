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
