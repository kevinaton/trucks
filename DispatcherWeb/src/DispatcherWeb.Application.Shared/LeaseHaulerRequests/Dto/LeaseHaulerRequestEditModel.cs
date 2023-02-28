using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class LeaseHaulerRequestEditModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public int LeaseHaulerId { get; set; }

        public int? Available { get; set; }

        public int? Approved { get; set; }

        public List<int?> Trucks { get; set; }

        public List<int?> Drivers { get; set; }
    }
}
