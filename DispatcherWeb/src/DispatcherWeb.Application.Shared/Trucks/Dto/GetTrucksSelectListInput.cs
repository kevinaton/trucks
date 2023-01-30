using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetTrucksSelectListInput : GetSelectListInput
    {
        public bool InServiceOnly { get; set; }
        public bool ActiveOnly { get; set; }

		public bool AllOffices { get; set; }
		public int? OfficeId { get; set; }

		public bool ExcludeTrailers { get; set; }
		//public bool ExcludeLeaseHaulers { get; set; }
        public bool IncludeLeaseHaulerTrucks { get; set; }
        public int? OrderLineId { get; set; }
    }
}
