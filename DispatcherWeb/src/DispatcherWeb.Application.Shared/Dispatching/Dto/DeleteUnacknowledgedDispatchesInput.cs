using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DeleteUnacknowledgedDispatchesInput
    {
		public int OrderLineId { get; set; }
		public int TruckId { get; set; }

	}
}
