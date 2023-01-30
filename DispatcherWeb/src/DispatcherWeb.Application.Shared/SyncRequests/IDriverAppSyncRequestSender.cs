using System.Threading.Tasks;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.SyncRequests
{
    public interface IDriverAppSyncRequestSender
    {
        Task SendSyncRequestAsync(SyncRequest syncRequest);
    }
}
