﻿namespace DispatcherWeb.Services.Dto
{
    public class GetServicePricingInput
    {
        public int ServiceId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? QuoteServiceId { get; set; }
    }
}
