namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineLoadAtIdInput
    {
        public int OrderLineId { get; set; }
        public int? LoadAtId { get; set; }
    }
}
