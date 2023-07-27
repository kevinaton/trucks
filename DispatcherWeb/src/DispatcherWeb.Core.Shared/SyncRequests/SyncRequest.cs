using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.SyncRequests.Entities;
using Newtonsoft.Json;

namespace DispatcherWeb.SyncRequests
{
    public partial class SyncRequest
    {

        //public static partial class Entities
        //{
        //    //public const string Dispatch = "Dispatch";
        //    //public const string EmployeeTime = "EmployeeTime";
        //    //public const string DriverAssignment = "DriverAssignment";
        //}

        public SyncRequest()
        {
        }

        public SyncRequest UpdateChangesFromReferences()
        {
            foreach (var change in Changes.OfType<ISyncRequestChangeDetail>())
            {
                if (change.Entity is IChangedEntity changedDriverAppEntity)
                {
                    changedDriverAppEntity.UpdateFromEntityReference();
                }
            }

            return this;
        }

        public List<SyncRequestChangeDetailAbstract> Changes { get; set; } = new List<SyncRequestChangeDetailAbstract>();

        [JsonIgnore]
        public bool IgnoreForCurrentUser { get; set; }

        [JsonIgnore]
        public int? IgnoreForDeviceId { get; set; }

        [JsonIgnore]
        public string LogMessage { get; set; }

        public SyncRequest AddChange<TEntity>(EntityEnum entityType, TEntity changedEntity, ChangeType changeType = ChangeType.Modified) where TEntity : ChangedEntityAbstract
        {
            if (!Changes.OfType<SyncRequestChangeDetail<TEntity>>().Any(x => x.Entity.IsSame(changedEntity)))
            {
                Changes.Add(new SyncRequestChangeDetail<TEntity>(entityType, changedEntity, changeType));
            }
            return this;
        }

        //public SyncRequest AddChange<TKey>(EntityEnum entity, TKey id, ChangeType changeType = ChangeType.Modified)
        //{
        //    Changes.Add(new SyncRequestChangeDetail<TKey>(entity, id, changeType));
        //    return this;
        //}

        public SyncRequest AddChanges<TEntity>(EntityEnum entityType, IEnumerable<TEntity> changedEntities, ChangeType changeType = ChangeType.Modified) where TEntity : ChangedEntityAbstract
        {
            foreach (var changedEntity in changedEntities.ToList())
            {
                if (!Changes.OfType<SyncRequestChangeDetail<TEntity>>().Any(x => x.Entity.IsSame(changedEntity)))
                {
                    Changes.Add(new SyncRequestChangeDetail<TEntity>(entityType, changedEntity, changeType));
                }
            }
            return this;
        }

        //public SyncRequest AddChangesById<TKey>(EntityEnum entity, IEnumerable<TKey> ids, ChangeType changeType = ChangeType.Modified)
        //{
        //    foreach (var id in ids.Distinct().ToList())
        //    {
        //        Changes.Add(new SyncRequestChangeDetail<TKey>(entity, id, changeType));
        //    }
        //    return this;
        //}

        //public SyncRequest AddChangeById(EntityEnum entity, int id, ChangeType changeType = ChangeType.Modified)
        //{
        //    Changes.Add(new SyncRequestChangeDetail<int>(entity, id, changeType));
        //    return this;
        //}

        //public SyncRequest AddChangesById(EntityEnum entity, IEnumerable<int> ids, ChangeType changeType = ChangeType.Modified)
        //{
        //    return AddChangesById<int>(entity, ids, changeType);
        //}

        //public SyncRequest AddChange(SyncRequestChangeDetailAbstract change)
        //{
        //    Changes.Add(change);
        //    return this;
        //}

        /// <summary>
        /// Set this flag to send the sync request to everyone except the user that made the change
        /// </summary>
        public SyncRequest SetIgnoreForCurrentUser(bool val)
        {
            IgnoreForCurrentUser = val;
            return this;
        }

        public SyncRequest SetIgnoreForDeviceId(int? val)
        {
            IgnoreForDeviceId = val;
            return this;
        }

        public SyncRequest AddLogMessage(string val)
        {
            if (!string.IsNullOrEmpty(LogMessage))
            {
                LogMessage += "\n";
            }
            LogMessage += val;
            return this;
        }
    }
}
