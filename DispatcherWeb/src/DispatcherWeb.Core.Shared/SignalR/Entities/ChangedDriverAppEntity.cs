using Abp.Domain.Entities.Auditing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.SignalR.Entities
{
    public class ChangedDriverAppEntity : ChangedDriverAppEntity<int>
    {
    }

    public class ChangedDriverAppEntity<TKey> : ChangedEntityId<TKey>, IChangedDriverAppEntity
    {
        public int? DriverId { get; set; } //no need to send driver app notification if driverId and OldDriverIdToNotify is null
        
        public DateTime LastUpdateDateTime { get; set; }

        [JsonIgnore]
        public int? OldDriverIdToNotify { get; set; } //no need to send two different driver app notifications if oldDrvierId is same as driverId

        public ChangedDriverAppEntity<TKey> SetOldDriverIdToNotify(int? oldDriverId)
        {
            OldDriverIdToNotify = oldDriverId;
            return this;
        }

        [JsonIgnore]
        public FullAuditedEntity<TKey> EntityReference { get; set; }

        public void UpdateFromEntityReference()
        {
            if (EntityReference != null)
            {
                Id = EntityReference.Id;
                this.SetLastUpdateDateTime(EntityReference);
            }
        }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedDriverAppEntity<TKey> other
                && other.DriverId.Equals(DriverId)
                && other.LastUpdateDateTime.Equals(LastUpdateDateTime)
                && other.OldDriverIdToNotify.Equals(OldDriverIdToNotify)
                && base.IsSame(obj);
        }
    }
}
