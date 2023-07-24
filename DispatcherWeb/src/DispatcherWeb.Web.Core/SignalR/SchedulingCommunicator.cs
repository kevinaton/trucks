using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using DispatcherWeb.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.SignalR
{
    public class SchedulingCommunicator : ISchedulingCommunicator, ITransientDependency
    {
        public ILogger Logger { get; set; }
        private readonly IHubContext<SchedulingHub> _hubContext;

        public SchedulingCommunicator(IHubContext<SchedulingHub> hubContext)
        {
            _hubContext = hubContext;
            Logger = NullLogger.Instance;
        }

        public async Task SendSyncScheduledTrucks(IReadOnlyList<IOnlineClient> clients)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                await signalRClient.SendAsync("syncScheduledTrucks");
            }
        }

        private IClientProxy GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _hubContext.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get dispatcher user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }
    }
}
