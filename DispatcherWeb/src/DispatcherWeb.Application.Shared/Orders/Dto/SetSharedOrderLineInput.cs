namespace DispatcherWeb.Orders.Dto
{
    public class SetSharedOrderLineInput
    {
        public int OrderLineId { get; set; }
        public int[] CheckedOfficeIds { get; set; }
    }
}
