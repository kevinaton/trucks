﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CancelOrEndAllDispatchesInput
    {
        public int? OrderLineId { get; set; }
        public int? TruckId { get; set; }
        //public DateTime? Date { get; set; }
        //public Shift? Shift { get; set; }
    }
}
