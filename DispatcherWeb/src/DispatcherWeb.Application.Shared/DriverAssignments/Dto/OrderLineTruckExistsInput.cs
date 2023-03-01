using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class OrderLineTruckExistsInput
    {
        public OrderLineTruckExistsInput()
        {
        }

        public OrderLineTruckExistsInput(int truckId, DateTime date, Shift? shift)
        {
            TruckId = truckId;
            Date = date;
            Shift = shift;
        }

        public int TruckId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
    }
}
