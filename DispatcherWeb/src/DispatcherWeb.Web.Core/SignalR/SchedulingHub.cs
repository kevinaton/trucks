using Abp.AspNetCore.SignalR.Hubs;
using Abp.Dependency;
using Abp.RealTime;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Web.SignalR
{
    public class SchedulingHub : OnlineClientHubBase, ITransientDependency
    {
        public SchedulingHub(
            IOnlineClientManager<SchedulingSignalRChannel> onlineClientManager,
            IOnlineClientInfoProvider clientInfoProvider)
            : base(onlineClientManager, clientInfoProvider)
        {
        }
    }
}