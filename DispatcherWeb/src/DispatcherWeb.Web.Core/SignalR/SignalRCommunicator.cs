using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using DispatcherWeb.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.SignalR
{
    public class SignalRCommunicator : ISignalRCommunicator, ITransientDependency
    {
        public ILogger Logger { get; set; }
        private readonly IHubContext<SignalRHub> _signalrHub;

        public SignalRCommunicator(IHubContext<SignalRHub> signalrHub)
        {
            _signalrHub = signalrHub;
            Logger = NullLogger.Instance;
        }

        public async Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                await signalRClient.SendAsync("syncRequest", syncRequest);
            }
        }

        private IClientProxy GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _signalrHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get dispatcher user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }
    }
}
