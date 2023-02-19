using DispatcherWeb.SignalR.Entities;

namespace DispatcherWeb.SignalR
{
    public interface ISyncRequestChangeDetail
    {
        ChangedEntityAbstract Entity { get; }
        EntityEnum EntityType { get; }
        ChangeType ChangeType { get; }
    }
}
