using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CreateDispatchesForDateShiftInput
    {
		public DateTime DeliveryDate { get; set; }
		public Shift? Shift { get; set; }
        public int[] OfficeIds { get; set; }
        public bool SendAllOrders { get; set; }
    }
}
