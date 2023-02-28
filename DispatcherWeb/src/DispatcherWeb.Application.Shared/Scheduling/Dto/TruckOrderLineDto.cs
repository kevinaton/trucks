using System;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Scheduling.Dto
{
    public class TruckOrderLineDto
    {
        public int OrderLineId { get; set; }
        public int OrderId { get; set; }
        public string Customer { get; set; }
        public DesignationEnum Designation { get; set; }
        public decimal Utilization { get; set; }
        public DateTime? StartTime => TruckStartTime ?? OrderLineStartTime;
        public DateTime? OrderLineStartTime { get; set; }
        public DateTime? TruckStartTime { get; set; }
        public DateTime? Time { get; set; }
        public string LoadAtNamePlain { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToNamePlain { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string Item { get; set; }
        public string MaterialUom { get; set; }
        public string FreightUom { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public DateTime? SharedDateTime { get; set; }
        public string DriverName { get; set; }
        public int? DriverId { get; set; }

        public string QuantityFormatted
        {
            get
            {
                var material = $"{MaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {MaterialUom}";
                var freight = $"{FreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {FreightUom}";

                if (Designation == DesignationEnum.MaterialOnly)
                {
                    return material;
                }

                if (Designation == DesignationEnum.FreightAndMaterial)
                {
                    if (MaterialUom == FreightUom)
                    {
                        return material;
                    }

                    return material + Environment.NewLine + freight;
                }

                return freight;
            }
        }
    }
}
