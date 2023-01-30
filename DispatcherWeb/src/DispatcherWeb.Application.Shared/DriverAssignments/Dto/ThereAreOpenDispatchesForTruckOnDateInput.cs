using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class ThereAreOpenDispatchesForTruckOnDateInput
    {
		public int TruckId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Shift? Shift { get; set; }
    }
}
