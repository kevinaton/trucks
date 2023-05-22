using System;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportTicketDto
    {
        public DateTime? TicketDateTime { get; set; }
        public string Item { get; set; }
        public string CustomerName { get; set; }
        public string JobNumber { get; set; } //?
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public decimal FreightRateToPayDrivers { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public decimal DriverPayRate { get; set; }
        public int TimeClassificationId { get; set; }
        public string TimeClassificationName { get; set; }
        public bool IsProductionPay { get; set; }
        public DriverIsPaidForLoadBasedOnEnum DriverIsPaidForLoadBasedOn { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }
    }
}
