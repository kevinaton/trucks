using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class LeaseHaulerSelectionModel
    {
        public List<LeaseHaulerSelectionRowDto> Rows { get; set; }
        public List<LeaseHaulerSelectionTruckDto> Trucks { get; set; }
        public List<SelectListDto> LeaseHaulers { get; set; }
        public List<LeaseHaulerSelectionDriverDto> Drivers { get; set; }
    }
}
