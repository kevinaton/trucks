using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetDriverInfoInput
    {
		public Guid AcknowledgeGuid { get; set; }
		public bool EditTicket { get; set; }
        public bool DoNotAcknowledge { get; set; }
	}
}
