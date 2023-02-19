using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedEmployeeTime : ChangedDriverAppEntity
    {
        public long UserId { get; set; }
        public int? TruckId { get; set; }
        public Guid? Guid { get; set; }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedEmployeeTime other
                && other.UserId.Equals(UserId)
                && other.TruckId.Equals(TruckId)
                && other.Guid.Equals(Guid)
                && base.IsSame(obj);
        }
    }
}
