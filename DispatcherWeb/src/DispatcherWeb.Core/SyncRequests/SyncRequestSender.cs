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
            syncRequest.UpdateChangesFromReferences();
            if (!syncRequest.Changes.Any())
            {
                return;
            }

            var clients = _onlineClientManager.GetAllClients()
                .Where(x => x.TenantId == Session.TenantId)
                .WhereIf(syncRequest.IgnoreForCurrentUser, x => x.UserId != Session.UserId)
                .ToList();

            await _signalRCommunicator.SendSyncRequest(clients, syncRequest);

            await _driverAppSyncRequestSender.SendSyncRequestAsync(syncRequest);
        }
    }
}
