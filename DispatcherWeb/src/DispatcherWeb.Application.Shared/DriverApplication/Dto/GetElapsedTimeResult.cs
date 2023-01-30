using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class GetElapsedTimeResult
    {
        //DriverApplication 1
        public DateTime ElapsedTime { get; set; }
        //DriverApplication 1 & 2
        public bool ClockIsStarted { get; set; }

        //DriverApplication 2:
        public DateTime? LastClockStartTime { get; set; }
        public double CommittedElapsedSeconds { get; set; }
        public DateTime CommittedElapsedSecondsForDay { get; set; }
    }
}
