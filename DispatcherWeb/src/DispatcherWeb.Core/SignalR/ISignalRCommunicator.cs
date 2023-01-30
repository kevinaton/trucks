using Abp.RealTime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.SignalR
{
    public interface ISignalRCommunicator
    {
        Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest);
    }
}
