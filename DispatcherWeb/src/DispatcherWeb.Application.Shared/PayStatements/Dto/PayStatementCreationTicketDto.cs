using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementCreationTicketDto
    {
        public int TicketId { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public int DriverId { get; set; }
        public long? UserId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public DateTime TicketCreationTime { get; set; }
    }
}
