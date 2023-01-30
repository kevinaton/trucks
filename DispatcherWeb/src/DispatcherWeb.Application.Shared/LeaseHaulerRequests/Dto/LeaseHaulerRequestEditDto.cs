using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
