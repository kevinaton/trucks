using DispatcherWeb.Common.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders.TaxDetails;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineEditDto : IOrderLineTaxDetails
    {
        public int? Id { get; set; }

        public int OrderId { get; set; }

        public int? QuoteId { get; set; }

        public int LineNumber { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialPricePerUnit { get; set; }

        public decimal? FreightPricePerUnit { get; set; }

        public bool IsMaterialPricePerUnitOverridden { get; set; }

        public bool IsFreightPricePerUnitOverridden { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public bool IsTaxable { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        
        [JsonIgnore]
        public string LoadAtNamePlain { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        
        [JsonIgnore]
        public string DeliverToNamePlain { get; set; }

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public string DesignationName => Designation.GetDisplayName();

        public decimal MaterialPrice { get; set; }
        
        public decimal FreightPrice { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public double? NumberOfTrucks { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public StaggeredTimeKind StaggeredTimeKind { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        public bool CanOverrideTotals { get; set; }

        public bool IsMultipleLoads { get; set; }

        public bool ProductionPay { get; set; }
        public bool HasQuoteBasedPricing { get; set; }
        public bool HasTickets { get; set; }
        public bool HasOpenDispatches { get; set; }
        public bool IsTimeStaggeredForTrucks { get; set; }
        public DateTime? FirstStaggeredTimeOnJob { get; set; }
        public int? StaggeredTimeInterval { get; set; }
        public bool UpdateStaggeredTime { get; set; }
    }
}
