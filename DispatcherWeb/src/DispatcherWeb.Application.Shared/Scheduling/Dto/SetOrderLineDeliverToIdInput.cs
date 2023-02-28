namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineDeliverToIdInput
    {
        public int OrderLineId { get; set; }
        public int? DeliverToId { get; set; }
    }
}
