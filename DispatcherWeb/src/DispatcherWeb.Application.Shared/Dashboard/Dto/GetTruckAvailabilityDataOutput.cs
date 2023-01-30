using System.Collections.Generic;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTruckAvailabilityDataOutput
    {
        public int Available { get; set; }
        public int OutOfService { get; set; }
    }
}
