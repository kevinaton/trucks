using Abp.RealTime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.SignalR
{
    public interface ISchedulingCommunicator
    {
        Task SendSyncScheduledTrucks(IReadOnlyList<IOnlineClient> clients);
    }
}