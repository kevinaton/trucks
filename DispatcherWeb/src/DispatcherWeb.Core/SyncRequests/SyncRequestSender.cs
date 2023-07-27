using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.RealTime;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.SyncRequests
{
    public class SyncRequestSender : ISyncRequestSender, ITransientDependency
    {
        private readonly IOnlineClientManager<DispatcherSignalRChannel> _onlineClientManager;
        private readonly ISignalRCommunicator _signalRCommunicator;
        private readonly IDriverAppSyncRequestSender _driverAppSyncRequestSender;

        public SyncRequestSender(
            IOnlineClientManager<DispatcherSignalRChannel> onlineClientManager,
            ISignalRCommunicator signalRCommunicator,
            AspNetZeroAbpSession session,
            IDriverAppSyncRequestSender driverAppSyncRequestSender
            )
        {
            _onlineClientManager = onlineClientManager;
            _signalRCommunicator = signalRCommunicator;
            Session = session;
            _driverAppSyncRequestSender = driverAppSyncRequestSender;
        }

        public AspNetZeroAbpSession Session { get; }

        public async Task SendSyncRequest(SyncRequest syncRequest)
        {
            await SendRequest(syncRequest);
            await _driverAppSyncRequestSender.SendSyncRequestAsync(syncRequest);
        }

        public async Task SendDataSyncRequest(SyncRequest syncRequest)
        {
            await SendRequest(syncRequest);
        }

        private async Task SendRequest(SyncRequest request)
        {
            request.UpdateChangesFromReferences();
            if (!request.Changes.Any())
            {
                return;
            }

            var clients = _onlineClientManager.GetAllClients()
                .Where(x => x.TenantId == Session.TenantId)
                .WhereIf(request.IgnoreForCurrentUser, x => x.UserId != Session.UserId)
                .ToList();

            await _signalRCommunicator.SendSyncRequest(clients, request);
        }
    }
}
