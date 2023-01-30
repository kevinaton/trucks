using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class LeaseHaulerRequestDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }
        public string LeaseHauler { get; set; }
        public DateTime? Sent { get; set; }
        public int? Available { get; set; }
        public int? Approved { get; set; }
        public int? Scheduled { get; set; }
    }
}
