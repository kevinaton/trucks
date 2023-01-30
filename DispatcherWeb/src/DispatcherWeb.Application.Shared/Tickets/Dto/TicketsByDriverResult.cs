using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketsByDriverResult
    {
        public List<OrderLineDto> OrderLines { get; set; }
        public List<DriverDto> Drivers { get; set; }
        public List<TicketDto> Tickets { get; set; }
        public List<DriverAssignmentDto> DriverAssignments { get; set; }
        public bool HasOpenOrders { get; set; }
        public List<TruckDto> Trucks { get; set; }
        public List<LeaseHaulerDto> LeaseHaulers { get; set; }
        public DailyFuelCostDto DailyFuelCost { get; set; }

        public class OrderLineDto
        {
            public int Id { get; set; }
            public bool IsComplete { get; set; }
            public bool IsCancelled { get; set; }
            public Shift? Shift { get; set; }
            public List<OrderLineTruckDto> OrderLineTrucks { get; set; }
            public int OrderId { get; set; }
            public string JobNumber { get; set; }
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public DateTime? OrderDate { get; set; }
            public int? LoadAtId { get; set; }
            public int? DeliverToId { get; set; }
            public string LoadAtName => LoadAt?.FormattedAddress;
            [JsonIgnore]
            public LocationNameDto LoadAt { get; set; }
            public string DeliverToName => DeliverTo?.FormattedAddress;
            [JsonIgnore]
            public LocationNameDto DeliverTo { get; set; }
            public int ServiceId { get; set; }
            public string ServiceName { get; set; }
            public DesignationEnum Designation { get; set; }
            [JsonIgnore]
            public int? MaterialUomId { get; set; }
            [JsonIgnore]
            public string MaterialUomName { get; set; }
            [JsonIgnore]
            public int? FreightUomId { get; set; }
            [JsonIgnore]
            public string FreightUomName { get; set; }
            public string UomName { get; set; }
            public int? UomId { get; set; }
            public decimal? FreightRate { get; set; }
            public decimal? MaterialRate { get; set; }
            public decimal? FuelSurchargeRate { get; set; }
            public decimal MaterialTotal { get; set; }
            public decimal FreightTotal { get; set; }
            public bool IsMaterialTotalOverridden { get; set; }
            public bool IsFreightTotalOverridden { get; set; }
        }

        public class DriverDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public bool IsExternal { get; set; }
            public int? LeaseHaulerId { get; set; }
        }

        public class TruckDto
        {
            public int Id { get; set; }
            public string TruckCode { get; set; }
            public bool IsActive { get; set; }
            public int? LeaseHaulerId { get; set; }
            public int? DefaultDriverId { get; set; }
        }

        public class TicketDto
        {
            public int Id { get; set; }
            public int? OrderLineId { get; set; }
            public string TicketNumber { get; set; }
            public DateTime? TicketDateTime { get; set; }
            public decimal Quantity { get; set; }
            public int? UomId { get; set; }
            public string UomName { get; set; }
            public int? TruckId { get; set; }
            public string TruckCode { get; set; } //only as a fallback value when TruckId is null or doesn't belong to a real truck
            public int? DriverId { get; set; }
            public Guid? TicketPhotoId { get; set; }
            public int? ReceiptLineId { get; set; }
            public bool IsReadOnly { get; set; }
            public bool IsVerified { get; set; }
            public int? CarrierId { get; set; }
        }

        public class OrderLineTruckDto
        {
            public int? DriverId { get; set; }
            public int TruckId { get; set; }
            public int Id { get; set; }
            public string DriverNote { get; set; }
        }

        public class DriverAssignmentDto
        {
            public int Id { get; set; }
            public Shift? Shift { get; set; }
            public int TruckId { get; set; }
            public int? DriverId { get; set; }
        }

        public class LeaseHaulerDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DailyFuelCostDto
        {
            public DateTime Date { get; set; }
            public decimal Cost { get; set; }
        }
    }
}
