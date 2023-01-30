using System;

namespace DispatcherWeb.DriverApp.Tickets.Dto
{
    public class TicketDto
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public string TicketNumber { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public string TicketPhotoFilename { get; set; }
        public int? LoadId { get; set; }
        public int? DispatchId { get; set; }
    }
}
