using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
	public class HasDispatchesResult
	{
		public string TruckCode { get; set; }
		public bool Unacknowledged { get; set; }
		public bool AcknowledgedOrLoaded { get; set; }
	}
}
