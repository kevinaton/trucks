using Abp.Domain.Entities.Auditing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.SignalR.Entities
{
    public class ChangedDriverAppEntity : ChangedDriverAppEntity<int>
    {
    }

    public class ChangedDriverAppEntity<TKey> : ChangedEntityId<TKey>, IChangedDriverAppEntity
    {
        public int? DriverId { get; set; } //no need to send driver app notification if driverId and OldDriverIdToNotify is null

        //[JsonIgnore]
        public List<int> DriverIds { get; set; } = new List<int>(); //can be used for entities affecting multiple drivers

        public DateTime LastUpdateDateTime { get; set; }

        [JsonIgnore]
        public int? OldDriverIdToNotify { get; set; } //no need to send two different driver app notifications if oldDrvierId is same as driverId

        public ChangedDriverAppEntity<TKey> SetOldDriverIdToNotify(int? oldDriverId)
        {
            OldDriverIdToNotify = oldDriverId;
            return this;
        }

        public ChangedDriverAppEntity<TKey> AddDriverIds(IEnumerable<int> driverIds)
        {
            DriverIds = DriverIds ?? new List<int>();
            DriverIds.AddRange(driverIds);
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

        private string GetSortedDriverIdsAsString()
        {
            return DriverIds == null ? null : string.Join(",", DriverIds.OrderBy(x => x).Select(x => x.ToString()));
        }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedDriverAppEntity<TKey> other
                && other.DriverId.Equals(DriverId)
                && other.GetSortedDriverIdsAsString() == GetSortedDriverIdsAsString()
                && other.LastUpdateDateTime.Equals(LastUpdateDateTime)
                && other.OldDriverIdToNotify.Equals(OldDriverIdToNotify)
                && base.IsSame(obj);
        }
    }
}
