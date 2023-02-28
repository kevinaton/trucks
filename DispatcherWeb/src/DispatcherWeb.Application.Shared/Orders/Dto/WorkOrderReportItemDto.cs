using DispatcherWeb.Common.Dto;
using DispatcherWeb.Orders.TaxDetails;
using Newtonsoft.Json;
using System;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportItemDto : IOrderLineTaxDetails
    {
        public int LineNumber { get; set; }
        public string ServiceName { get; set; }
        public bool IsTaxable { get; set; }
        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public decimal FreightPrice { get; set; }
        public decimal MaterialPrice { get; set; }
        public bool IsMaterialTotalOverridden { get; set; }
        public bool IsFreightTotalOverridden { get; set; }
        public decimal? Rate => FreightPricePerUnit.HasValue || MaterialPricePerUnit.HasValue ? (FreightPricePerUnit ?? 0) + (MaterialPricePerUnit ?? 0) : (decimal?)null;
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public double NumberOfTrucks { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public bool IsTimeStaggered { get; set; }


        public string ActualQuantityFormatted => GetQuantityFormatted(ActualQuantity, ActualQuantity);

        public string QuantityFormatted => GetQuantityFormatted(MaterialQuantity, FreightQuantity);

        private string GetQuantityFormatted(decimal? materialQuantityToUse, decimal? freightQuantityToUse)
        {
            var material = $"{materialQuantityToUse?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {MaterialUomName}";
            var freight = $"{freightQuantityToUse?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {FreightUomName}";

            if (Designation == DesignationEnum.MaterialOnly)
            {
                return material;
            }

            if (Designation == DesignationEnum.FreightAndMaterial)
            {
                if (MaterialUomName == FreightUomName)
                {
                    return material;
                }

                return material + Environment.NewLine + freight;
            }

            return freight;
        }
    }
}
