using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class SetDriverForTruckResult
    {
        public bool Success { get; set; }
        public bool OrderLineTruckExists { get; set; }
    }
}
