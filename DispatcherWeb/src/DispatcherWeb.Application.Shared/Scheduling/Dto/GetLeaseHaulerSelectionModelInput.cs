using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetLeaseHaulerSelectionModelInput
    {
        public int OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
    }
}
