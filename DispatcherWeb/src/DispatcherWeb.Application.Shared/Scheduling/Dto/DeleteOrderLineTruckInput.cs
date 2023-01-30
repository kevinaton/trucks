namespace DispatcherWeb.Scheduling.Dto
{
    public class DeleteOrderLineTruckInput
    {
        public int OrderLineTruckId { get; set; }
        public int OrderLineId { get; set; }

		public bool MarkAsDone { get; set; }
    }
}
