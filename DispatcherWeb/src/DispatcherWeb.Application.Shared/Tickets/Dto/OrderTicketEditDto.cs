using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Tickets.Dto
{
    public class OrderTicketEditDto
    {
        public int OrderLineId { get; set; }

        public int Id { get; set; }
        [StringLength(30)]
        public string TicketNumber { get; set; }

        public DateTime? TicketDateTime { get; set; }

        public decimal Quantity { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? TrailerId { get; set; }
        public string TrailerTruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
