namespace DispatcherWeb.Scheduling.Dto
{
	public class SchedulePageFeatures
	{
		public bool AllowSharedOrders { get; set; }
		public bool AllowMultiOffice { get; set; }
		public bool AllowSendingOrdersToDifferentTenant { get; set; }
		public bool AllowImportingTruxEarnings { get; set; }

        public bool LeaseHaulers { get; set; }
	}
}