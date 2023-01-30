using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;
using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class RawDispatchDto
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string TruckCode { get; set; }
        public string DriverLastFirstName { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Acknowledged { get; set; }
        public DateTime? Loaded { get; set; }
        public DateTime? Delivered { get; set; }
        public DispatchStatus Status { get; set; }
        public string CustomerName { get; set; }
        public string QuoteName { get; set; }
        public string JobNumber { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        public string LoadAtNamePlain { get; set; }
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        public string DeliverToNamePlain { get; set; }
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string Item { get; set; }
        public decimal? Quantity { get; set; }
        public string Uom { get; set; }
        public Guid Guid { get; set; }
        public bool IsMultipleLoads { get; set; }
    }
}
