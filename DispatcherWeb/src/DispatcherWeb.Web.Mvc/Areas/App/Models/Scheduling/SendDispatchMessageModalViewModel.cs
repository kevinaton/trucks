using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class SendDispatchMessageModalViewModel
    {
        public SendDispatchMessageModalViewModel(SendDispatchMessageDto sendDispatchMessageDto, bool canAddDispatchBasedOnTime, int? selectedOrderLineTruckId)
        {
            OrderLineId = sendDispatchMessageDto.OrderLineId;
            OrderLineTrucks = sendDispatchMessageDto.OrderLineTrucks
                .Select(x => new OrderLineTruckSelectListItem
                {
                    Text = $"{x.TruckCode} - {x.DriverName}",
                    Value = x.OrderLineTruckId.ToString(),
                    Selected = selectedOrderLineTruckId.HasValue
                        ? selectedOrderLineTruckId == x.OrderLineTruckId
                        : true,
                    HasPhone = x.HasPhone,
                    OrderLineTruckId = x.OrderLineTruckId
                }).ToList();
            Message = sendDispatchMessageDto.Message;
            IsMultipleLoads = sendDispatchMessageDto.IsMultipleLoads;
            CanAddDispatchBasedOnTime = canAddDispatchBasedOnTime;
        }

        public int OrderLineId { get; set; }
        public IList<OrderLineTruckSelectListItem> OrderLineTrucks { get; set; }
        public string Message { get; }
        public bool IsMultipleLoads { get; set; }
        public bool CanAddDispatchBasedOnTime { get; set; }


        public class OrderLineTruckSelectListItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public bool Selected { get; set; }
            public bool HasPhone { get; set; }
            public int OrderLineTruckId { get; set; }
        }
    }
}
