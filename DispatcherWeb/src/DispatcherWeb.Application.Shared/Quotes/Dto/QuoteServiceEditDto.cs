using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Infrastructure;
using Newtonsoft.Json;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteServiceEditDto
    {
        public int? Id { get; set; }

        public int QuoteId { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }

        public decimal? PricePerUnit { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal? LeaseHaulerRate { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }

        public bool ProductionPay { get; set; }

        public bool LoadBased { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public List<QuoteServiceVehicleCategoryDto> VehicleCategories { get; set; }
    }
}
