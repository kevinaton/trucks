using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class AddSharedTruckInput
    {
        public int TruckId { get; set; }

        public int OfficeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

		public Shift[] Shifts { get; set; }
    }
}
