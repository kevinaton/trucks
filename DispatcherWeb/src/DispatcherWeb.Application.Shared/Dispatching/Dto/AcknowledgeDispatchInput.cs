using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class AcknowledgeDispatchInput
    {
        public int DispatchId { get; set; }
        public DriverApplicationActionInfo Info { get; set; }
    }
}
