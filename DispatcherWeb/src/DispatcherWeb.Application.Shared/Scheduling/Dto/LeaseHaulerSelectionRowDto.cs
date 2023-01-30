using DispatcherWeb.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class LeaseHaulerSelectionRowDto
    {
        public int Id { get; set; }
        public int LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
