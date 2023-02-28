namespace DispatcherWeb.Tickets.Dto
{
    public class GetTruckForTicketDriverResult
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
    }
}
