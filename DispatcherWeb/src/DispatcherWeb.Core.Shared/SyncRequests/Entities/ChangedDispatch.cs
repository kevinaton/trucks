namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedDispatch : ChangedDriverAppEntity
    {

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedDispatch other
                //&& other.UserId.Equals(UserId)
                //&& other.TruckId.Equals(TruckId)
                && base.IsSame(obj);
        }
    }
}
