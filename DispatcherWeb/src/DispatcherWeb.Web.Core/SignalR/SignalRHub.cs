using Abp.AspNetCore.SignalR.Hubs;
using Abp.Dependency;
using Abp.RealTime;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Web.SignalR
{
    public class SignalRHub : OnlineClientHubBase, ITransientDependency
    {
        public SignalRHub(IOnlineClientManager<DispatcherSignalRChannel> onlineClientManager, IOnlineClientInfoProvider clientInfoProvider) : base(onlineClientManager, clientInfoProvider)
        {
        }
    }
}