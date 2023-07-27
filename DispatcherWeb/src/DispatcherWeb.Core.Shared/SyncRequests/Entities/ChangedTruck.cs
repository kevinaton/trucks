namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedTruck : ChangedEntityId<int>
    {
        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedTruck other && base.IsSame(obj);
        }
    }
}