using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverActivityDetailReportLoadDto
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DispatchId { get; set; }
        public int OrderLineId { get; set; }
        public string CustomerName { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string TicketNumber { get; set; }
        public decimal? Quantity { get; set; }
        public string UomName { get; set; }
        public DateTime? LoadTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public TimeSpan? CycleTime { get; set; }
        public string JobNumber { get; set; }
        public string ProductOrService { get; set; }
    }
}
