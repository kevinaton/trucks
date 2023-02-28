using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.RealTime;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.SignalR
{
    public interface ISignalRCommunicator
    {
        Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest);
    }
}
