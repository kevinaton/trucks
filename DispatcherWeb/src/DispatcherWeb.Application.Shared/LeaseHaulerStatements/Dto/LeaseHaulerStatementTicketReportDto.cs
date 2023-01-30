using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;
using System;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class LeaseHaulerStatementTicketReportDto
    {
        public DateTime? OrderDate { get; set; }
        public Shift? Shift { get; set; }
        public string ShiftName { get; set; }
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public string TicketNumber { get; set; }
        public string CarrierName { get; set; }
        public string TruckCode { get; set; }
        public string DriverName { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string UomName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Rate { get; set; }
        public decimal BrokerFee { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal ExtendedAmount { get; set; }
        public DateTime? TicketDateTime { get; set; }
    }
}
