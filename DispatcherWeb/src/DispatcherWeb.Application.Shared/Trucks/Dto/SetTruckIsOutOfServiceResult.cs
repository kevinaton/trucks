using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Trucks.Dto
{
    public class SetTruckIsOutOfServiceResult
    {
        public bool ThereWereAssociatedOrders { get; set; }
		public bool ThereWereCanceledDispatches { get; set; }
		public bool ThereWereNotCanceledDispatches { get; set; }
	}
}
