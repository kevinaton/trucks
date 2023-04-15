using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class LeaseHaulerRequestEditDto
    {
        public int Id { get; set; }

        public DateTime? Date { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }

        public int LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }

        public string Comments { get; set; }

        public int? Available { get; set; }
        public int? Approved { get; set; }
        public List<AvailableTrucksTruckEditDto> Trucks { get; set; }

        public bool? RequestFromScheduler { get;set; }
    }
}
