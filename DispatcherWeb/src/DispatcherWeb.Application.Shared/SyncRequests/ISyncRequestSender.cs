using System.Threading.Tasks;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.SyncRequests
{
    public interface ISyncRequestSender
    {
        Task SendSyncRequest(SyncRequest syncRequest);
    }
}
