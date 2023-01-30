namespace DispatcherWeb.Services.Dto
{
    public class ServicePricingDto
    {
        public decimal? PricePerUnit { get; set; }
        public decimal? FreightRate { get; set; }
        public bool HasPricing { get; set; }
        public QuoteServicePricingDto QuoteBasedPricing { get; set; }
    }
}
