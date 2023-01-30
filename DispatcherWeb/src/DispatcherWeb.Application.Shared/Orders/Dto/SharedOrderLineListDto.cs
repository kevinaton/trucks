using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class SharedOrderLineListDto
    {
        public int OrderLineId { get; set; }
        public List<SharedOrderDto> SharedOrders { get; set; }
    }
}
