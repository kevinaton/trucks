namespace DispatcherWeb.Services.Dto
{
    public class GetServicePricingInput
    {
        public int ServiceId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? LoadAtId { get; set; }
        public int? DeliverToId { get; set; }
        public int? QuoteId { get; set; }
    }
}
