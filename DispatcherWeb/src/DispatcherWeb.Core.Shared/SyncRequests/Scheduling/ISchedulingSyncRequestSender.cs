using System.Threading.Tasks;

namespace DispatcherWeb.SyncRequests.Scheduling
{
    public interface ISchedulingSyncRequestSender
    {
        Task SendSyncScheduledTrucksRequest();
    }
}