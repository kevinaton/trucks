using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Drivers.Dto
{
    public class DriverTrucksDto
    {
        public List<DriverTruckDto> Trucks { get; set; }
        public List<string> TruckCodes => Trucks.Select(x => x.TruckCode).ToList();
    }
}
