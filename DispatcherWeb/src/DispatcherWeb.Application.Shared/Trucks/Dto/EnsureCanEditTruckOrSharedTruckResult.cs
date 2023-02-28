using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Trucks.Dto
{
    public class EnsureCanEditTruckOrSharedTruckResult
    {
        public List<SharedTruckDto> SharedTrucks { get; set; }
        public int? TruckOfficeId { get; set; }

        public int? GetLocationForDate(DateTime date, Shift? shift)
        {
            return SharedTrucks.FirstOrDefault(x => x.Date == date && x.Shift == shift)?.OfficeId ?? TruckOfficeId;
        }
    }
}
