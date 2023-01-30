using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class SharedOrderListDto
    {
        public int OrderId { get; set; }
        public List<SharedOrderDto> SharedOrders { get; set; }
    }
}
