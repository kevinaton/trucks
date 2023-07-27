using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Timing;
using Newtonsoft.Json;

namespace DispatcherWeb.SyncRequests.Entities
{
    public abstract class ChangedEntityAbstract
    {
        public virtual bool IsSame(ChangedEntityAbstract obj)
        {
            return true;
        }
    }

    public class ChangedEntityId<TKey> : ChangedEntityAbstract, IChangedEntity
    {
        public TKey Id { get; set; }

        public ChangedEntityId()
        {
        }

        public ChangedEntityId(TKey id)
        {
            Id = id;
        }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedEntityId<TKey> other
                && other.Id.Equals(Id)
                && base.IsSame(obj);
        }

        [JsonIgnore]
        public Entity<TKey> EntityReference { get; set; }

        public virtual void UpdateFromEntityReference()
        {
            if (EntityReference != null)
            {
                Id = EntityReference.Id;
            }
        }
    }


    public class ChangedEntityId : ChangedEntityId<int>
    {
        public ChangedEntityId()
        {
        }

        public ChangedEntityId(int id) : base(id)
        {
        }
    }
}
