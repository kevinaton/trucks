using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class ThereAreOpenDispatchesForDriverOnDateResult
    {
		public bool ThereAreUnacknowledgedDispatches { get; set; }
		public bool ThereAreAcknowledgedDispatches { get; set; }
    }
}
