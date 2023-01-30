using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;
using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementItemEditDto
    {
        public PayStatementItemKind ItemKind { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public DateTime? Date { get; set; }
        public int TimeClassificationId { get; set; }
        public string TimeClassificationName { get; set; }
        public bool IsProductionPay { get; set; }
        public decimal? DriverPayRate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }

        public string Item { get; set; }
        public string CustomerName { get; set; }
        public string JobNumber { get; set; }
        public string DeliverToNamePlain { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string LoadAtNamePlain { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public decimal FreightRate { get; set; }
        public int Id { get; set; }
    }
}
