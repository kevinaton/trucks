using System;
using System.Collections.Generic;

namespace DispatcherWeb.SignalR.Entities
{
    public interface IChangedDriverAppEntity
    {
        int? DriverId { get; set; }
        List<int> DriverIds { get; set; }
        DateTime LastUpdateDateTime { get; set; }
        int? OldDriverIdToNotify { get; set; }
        void UpdateFromEntityReference();
    }
}
