namespace DispatcherWeb.Orders.Dto
{
    public class SetSharedOrdersInput
    {
        public int OrderId { get; set; }
        public int[] CheckedOfficeIds { get; set; }
    }
}
