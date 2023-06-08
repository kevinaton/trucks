using System;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchListDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public string DriverLastFirstName { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Acknowledged { get; set; }
        public DateTime? Loaded { get; set; }
        public DateTime? Delivered { get; set; }
        public string Status { get; set; }
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
        public bool Cancelable { get; set; }
        public string ShortGuid { get; set; }
        public Guid Guid { get; set; }
        public DispatchStatus DispatchStatus { get; set; }
        public bool IsMultipleLoads { get; set; }
    }
}
