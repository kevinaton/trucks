using System;
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderTruckQueryResultDto
    {
        public string TruckCode { get; set; }
        public string UnitOfMeasure { get; set; }
        public int LoadsCount { get; set; }
        public decimal Quantity { get; set; }
        public IList<OrderTruckLoadQueryResultDto> Loads { get; set; }
        public int TruckId { get; set; }

        public class OrderTruckLoadQueryResultDto
        {
            public double?[] SourceCoordinates { get; set; }
            public double?[] DestinationCoordinates { get; set; }
            public int LoadId { get; set; }
            public int? DriverId { get; set; }
            public DateTime? SourceDateTime { get; set; }
            public DateTime? DestinationDateTime { get; set; }
        }
    }
}


