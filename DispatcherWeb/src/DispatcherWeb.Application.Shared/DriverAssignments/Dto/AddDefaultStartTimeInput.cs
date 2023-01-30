using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class AddDefaultStartTimeInput
    {
        public int OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public DateTime? DefaultStartTime { get; set; }
    }
}
