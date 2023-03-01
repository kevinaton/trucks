namespace DispatcherWeb.Dispatching.Dto
{
    public class DeleteUnacknowledgedDispatchesInput
    {
        public int OrderLineId { get; set; }
        public int TruckId { get; set; }

    }
}
