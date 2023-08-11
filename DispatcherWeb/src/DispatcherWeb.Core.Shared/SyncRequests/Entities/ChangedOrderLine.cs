namespace DispatcherWeb.SyncRequests.Entities
{
	public class ChangedOrderLine : ChangedEntityId<int>
	{
		public override bool IsSame(ChangedEntityAbstract obj)
		{
			return obj is ChangedOrderLine other && base.IsSame(obj);
		}
	}
}