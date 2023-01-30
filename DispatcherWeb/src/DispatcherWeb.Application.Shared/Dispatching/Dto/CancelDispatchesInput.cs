using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CancelDispatchesInput
    {
		public int? OrderLineId { get; set; }
		public int? TruckId { get; set; }
        public DateTime? Date { get; set; }
        public Shift? Shift { get; set; }

		public DispatchStatus[] CancelDispatchStatuses { get; set; }
	}
}
