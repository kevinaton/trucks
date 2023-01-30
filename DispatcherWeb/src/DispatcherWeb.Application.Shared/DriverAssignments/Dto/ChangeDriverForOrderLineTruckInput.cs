using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class ChangeDriverForOrderLineTruckInput
    {
        public int OrderLineTruckId { get; set; }
        public int DriverId { get; set; }
        public bool ReplaceExistingDriver { get; set; }
    }
}
