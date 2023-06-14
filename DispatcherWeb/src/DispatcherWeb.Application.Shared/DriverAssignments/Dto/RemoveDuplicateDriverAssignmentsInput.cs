using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class RemoveDuplicateDriverAssignmentsInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public int TruckId { get; set; }
    }
}
