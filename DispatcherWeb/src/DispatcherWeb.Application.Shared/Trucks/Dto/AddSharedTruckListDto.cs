using System;
using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class AddSharedTruckListDto
    {
        public int TruckId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SelectListDto> Offices { get; set; }
    }
}
