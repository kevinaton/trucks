﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverActivityDetailReportPageDto
    {
        public DateTime Date { get; set; }
        public int DriverId { get; set; }
        public long UserId { get; set; }
        public string DriverName { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public List<DriverActivityDetailReportEmployeeTimeDto> EmployeeTimes { get; set; }
        public List<DriverActivityDetailReportLoadDto> Loads { get; set; }
    }
}
