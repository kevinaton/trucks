using System.Collections.Generic;
using DispatcherWeb.Dispatching.Dto.DispatchSender;

namespace DispatcherWeb.Dispatching.Dto
{
    public class SendDispatchMessageDto
    {
        public int OrderLineId { get; set; }
        public string Message { get; set; }
        public IList<OrderLineTruckDto> OrderLineTrucks { get; set; }
        public bool IsMultipleLoads { get; set; }
    }
}
