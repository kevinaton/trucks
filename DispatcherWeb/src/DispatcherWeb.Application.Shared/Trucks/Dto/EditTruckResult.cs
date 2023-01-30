using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Trucks.Dto
{
    public class EditTruckResult
    {
		public int Id { get; set; }

		public bool ThereWereCanceledDispatches { get; set; }
		public bool ThereWereNotCanceledDispatches { get; set; }
		public bool ThereAreOrdersInTheFuture { get; set; }
        public bool ThereWereAssociatedOrders { get; set; }
        public int NeededBiggerNumberOfTrucks { get; set; }
    }
}
