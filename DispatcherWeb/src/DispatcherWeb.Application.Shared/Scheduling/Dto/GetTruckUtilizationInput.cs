using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetTruckUtilizationInput
    {
		public DateTime Date { get; set; }
		public Shift? Shift { get; set; }
		public int TruckId { get; set; }
    }
}
