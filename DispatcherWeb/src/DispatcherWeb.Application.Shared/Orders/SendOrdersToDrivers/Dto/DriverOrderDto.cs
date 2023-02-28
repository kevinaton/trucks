using System;
using System.Collections.Generic;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.Dto
{
    public class DriverOrderDto
    {
        public string DriverFullName => DriverFirstName + " " + DriverLastName;

        public string DriverFirstName { get; set; }

        public string DriverLastName { get; set; }

        public string EmailAddress { get; set; }

        public string CellPhoneNumber { get; set; }

        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }

        public DateTime DeliveryDate { get; set; }
        public string ShiftName { get; set; }

        public string TruckCode { get; set; }

        public List<OrderDto> Orders { get; set; }

        public class OrderDto
        {
            public int Id { get; set; }

            public string Directions { get; set; }

            public DateTime? OrderTime { get; set; }

            public IEnumerable<OrderLineDto> OrderLines { get; set; }

            public class OrderLineDto
            {
                public string Item { get; set; }
                public decimal? MaterialQuantity { get; set; }
                public decimal? FreightQuantity { get; set; }
                public string MaterialUomName { get; set; }
                public string FreightUomName { get; set; }
                public string LoadAtName => LoadAt?.FormattedAddress;
                [JsonIgnore]
                public LocationNameDto LoadAt { get; set; }
                public string DeliverToName => DeliverTo?.FormattedAddress;
                [JsonIgnore]
                public LocationNameDto DeliverTo { get; set; }
                public string Note { get; set; }
            }
        }
    }
}
