using System;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Tickets;
using Newtonsoft.Json;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverActivityDetailReportLoadDto : ITicketQuantity
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
        public string LoadTicket { get; set; }
        public decimal? FreightQuantityOrdered { get; set; }
        public decimal? MaterialQuantityOrdered { get; set; }
        public decimal Quantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public string UomName { get; set; }
        public string TrailerTruckCode { get; set; }
        public string VehicleCategory { get; set; }
        public DateTime? LoadTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public TimeSpan? CycleTime { get; set; }
        public string JobNumber { get; set; }
        public string ProductOrService { get; set; }

        public decimal? QuantityOrdered
        {
            get
            {
                var useMaterial = this.GetAmountTypeToUse().useMaterial;
                if (useMaterial)
                {
                    return MaterialQuantityOrdered;
                }
                else
                {
                    return FreightQuantityOrdered;
                }
            }
        }
    }
}
