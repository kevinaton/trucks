using System;
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderTrucksDto
    {
        public DateTime Earliest { get; set; }
        public DateTime Latest { get; set; }
        public IList<OrderTruckDto> OrderTrucks { get; set; } = new List<OrderTruckDto>();

        public override string ToString() => $"Earliest: {Earliest} Latest:{Latest} OrderTrucks:{OrderTrucks.Count}";

        public class OrderTruckDto
        {
            public int? TruckId { get; set; }
            public string TruckCode { get; set; }
            public int? LoadsCount { get; set; }
            public decimal? Quantity { get; set; }
            public string UnitOfMeasure { get; set; }
            public IList<TripCycleDto> TripCycles { get; set; } = new List<TripCycleDto>();
        }

        public class TripCycleDto
        {
            public string CycleId { get; set; }
            public int? DriverId { get; set; }
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
            public double?[] SourceCoordinates { get; set; }
            public double?[] DestinationCoordinates { get; set; }
            public TruckTripTypesEnum TruckTripType { get; set; }
            public string Label { get; set; }

            public override string ToString() => $"Type: {TruckTripType} Driver: {DriverId} Label: {Label} Time Range:{(StartDateTime.HasValue ? StartDateTime.Value.ToString("hh:mm:ss") : "~")} -> {(EndDateTime.HasValue ? EndDateTime.Value.ToString("hh:mm:ss") : "~")}";
        }

        public enum TruckTripTypesEnum
        {
            TripToLoadSite = 0,
            TripToDeliverySite = 1
        }
    }
}


