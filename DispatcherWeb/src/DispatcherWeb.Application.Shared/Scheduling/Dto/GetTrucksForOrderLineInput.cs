using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetTrucksForOrderLineInput : IGetScheduleInput
    {
        public int? OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int OrderLineId { get; set; }
        public string TruckCode { get; set; }
        public bool OnlyTrailers { get; set; }
        public bool? IsPowered { get; set; }
    }
}
