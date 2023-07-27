using System.Threading.Tasks;

namespace DispatcherWeb.SyncRequests
{
    public interface ISyncRequestSender
    {
        Task SendSyncRequest(SyncRequest syncRequest);
        Task SendDataSyncRequest(SyncRequest syncRequest);
    }
}
