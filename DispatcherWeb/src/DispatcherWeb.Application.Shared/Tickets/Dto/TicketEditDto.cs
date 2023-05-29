using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketEditDto
    {
        public int? OrderLineId { get; set; }
        public DesignationEnum? OrderLineDesignation { get; set; }
        public DateTime? OrderDate { get; set; }
        public int Id { get; set; }
        [StringLength(20)]
        public string TicketNumber { get; set; }

        public decimal Quantity { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public Shift? Shift { get; set; }
        public int? UomId { get; set; }
        public string UomName { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBilled { get; set; }
        public int? ReceiptLineId { get; set; }
        public int? LoadAtId { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public int? DeliverToId { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public bool? ReadOnly { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public bool? OrderLineIsProductionPay { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public string CannotEditReason { get; set; }
        public bool IsReadOnly { get; set; }
        public bool HasPayStatements { get; set; }
        public bool HasLeaseHaulerStatements { get; set; }
    }
}
