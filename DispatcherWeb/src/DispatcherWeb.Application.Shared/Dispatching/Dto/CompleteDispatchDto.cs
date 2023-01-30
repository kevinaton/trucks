using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CompleteDispatchDto
    {
        public DriverApplicationActionInfo Info { get; set; }
        public Guid Guid { get; set; }
        public bool? IsMultipleLoads { get; set; }
        public bool? ContinueMultiload { get; set; }
        public double? DestinationLatitude { get; set; }
		public double? DestinationLongitude { get; set; }

	}
}
