using System;

namespace DispatcherWeb.SignalR.Entities
{
    public interface IChangedDriverAppEntity
    {
        int? DriverId { get; set; }
        DateTime LastUpdateDateTime { get; set; }
        int? OldDriverIdToNotify { get; set; }
        void UpdateFromEntityReference();
    }
}
