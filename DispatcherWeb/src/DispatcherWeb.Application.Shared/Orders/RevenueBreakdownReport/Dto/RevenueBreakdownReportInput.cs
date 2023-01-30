using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class RevenueBreakdownReportInput
    {
		public int? CustomerId { get; set; }
		public int? OfficeId { get; set; }

		public DateTime DeliveryDateBegin { get; set; }
		public DateTime DeliveryDateEnd { get; set; }

		public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

		public int? ServiceId { get; set; }

		public Shift[] Shifts { get; set; }
	}
}
