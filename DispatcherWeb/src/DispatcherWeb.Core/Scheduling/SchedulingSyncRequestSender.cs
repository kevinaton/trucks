using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.RealTime;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SignalR;
using DispatcherWeb.SyncRequests.Scheduling;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling
{
    internal class SchedulingSyncRequestSender : ISchedulingSyncRequestSender, ITransientDependency
    {
        private readonly IOnlineClientManager<SchedulingSignalRChannel> _onlineClientManager;
        private readonly ISchedulingCommunicator _schedulingCommunicator;

        public SchedulingSyncRequestSender(
            IOnlineClientManager<SchedulingSignalRChannel> onlineClientManager, 
            ISchedulingCommunicator schedulingCommunicator,
            AspNetZeroAbpSession session)
        {
            _onlineClientManager = onlineClientManager;
            _schedulingCommunicator = schedulingCommunicator;
            Session = session;
        }

        public AspNetZeroAbpSession Session { get; }

        public async Task SendSyncScheduledTrucksRequest()
        {
            var clients = _onlineClientManager.GetAllClients()
                .Where(x => x.TenantId == Session.TenantId)
                .WhereIf(true, x => x.UserId != Session.UserId)
                .ToList();

            await _schedulingCommunicator.SendSyncScheduledTrucks(clients);
        }
    }
}
