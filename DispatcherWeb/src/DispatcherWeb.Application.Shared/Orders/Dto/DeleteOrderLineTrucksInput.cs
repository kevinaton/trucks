using System;

namespace DispatcherWeb.Orders.Dto
{
    public class DeleteOrderLineTrucksInput
    {
        public int? OrderLineId { get; set; }
        //or
        public int? TruckId { get; set; }
        public DateTime? Date { get; set; }
        public Shift? Shift { get; set; }

        public bool MarkAsDone { get; set; }
    }
}
