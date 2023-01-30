using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Tickets.Dto
{
    public class LookForExistingOrderLinesInput
	{
		[Required]
		public DateTime TicketDateTime { get; set; }

		[Required]
		public int CustomerId { get; set; }

		[Required]
		public int ServiceId { get; set; }

		[Required]
		public string TruckCode { get; set; }
    }
}
