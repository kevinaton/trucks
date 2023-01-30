using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetDriverDispatchesForCurrentUserInput
    {
        public DateTime? UpdatedAfterDateTime { get; set; }
        public Guid? DriverGuid { get; set; }
    }
}
