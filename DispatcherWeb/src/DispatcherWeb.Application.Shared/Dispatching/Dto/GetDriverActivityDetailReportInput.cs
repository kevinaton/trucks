using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetDriverActivityDetailReportInput
    {
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? DriverId { get; set; }
    }
}
