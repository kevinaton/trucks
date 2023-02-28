namespace DispatcherWeb.Invoices.Dto
{
    public class GetTicketDetailsByTicketNumberInput
    {
        public string TicketNumber { get; set; }
        public int CustomerId { get; set; }
    }
}
