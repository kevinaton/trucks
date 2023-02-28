using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers.Dto
{
    public class GetDriversFromOrderLineSelectListInput : GetSelectListInput
    {
        public int OrderLineId { get; set; }
        public bool IncludeSharedTrucks { get; set; }
    }
}
