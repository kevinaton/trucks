namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteSelectListInfoDto
    {
        public int? ProjectId { get; set; }
        public QuoteStatus Status { get; set; }
        public string ChargeTo { get; set; }
        public string PONumber { get; set; }
        public string SpectrumNumber { get; set; }
        public int? ContactId { get; set; }
        public string Directions { get; set; }
        public int? CustomerId { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public int? FuelSurchargeCalculationId { get; set; }
        public string FuelSurchargeCalculationName { get; set; }
        public decimal? BaseFuelCost { get; set; }
        public bool? CanChangeBaseFuelCost { get; set; }
    }
}
