using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetAddSharedTruckModelInput
    {
        public int TruckId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
