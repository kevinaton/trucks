using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketPrintOutDto
    {
        public string LogoPath { get; set; }
        public string LegalName { get; set; }
        public string LegalAddress { get; set; }
        public string BillingPhoneNumber { get; set; }


        public string TicketNumber { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public string MaterialUomName { get; set; }
        public string Note { get; set; }
    }
}
