using System;
using System.Collections.Generic;

namespace DispatcherWeb.SyncRequests.Entities
{
    public interface IChangedDriverAppEntity
    {
        int? DriverId { get; set; }
        List<int> DriverIds { get; set; }
        long? UserId { get; set; }
        DateTime LastUpdateDateTime { get; set; }
        int? OldDriverIdToNotify { get; set; }
        void UpdateFromEntityReference();
    }
}
