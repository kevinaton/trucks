using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class SetDriverForTruckInput
    {
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public DateTime Date { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
		public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public bool CreateNewDriverAssignment { get; set; }
	}
}
