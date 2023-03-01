using System;
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class LeaseHaulerSelectionDto
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public bool AddAllTrucks { get; set; }
        public List<LeaseHaulerSelectionRowDto> Rows { get; set; }
        public int? LeaseHaulerId { get; set; }
    }
}
