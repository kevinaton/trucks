using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Projects.Dto
{
    public class ProjectServiceDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string ServiceName { get; set; }


        public string MaterialUomName { get; set; }

        public string FreightUomName { get; set; }

        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();

        public string LoadAtNamePlain { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }

        public string DeliverToNamePlain { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }

        public decimal? PricePerUnit { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public string Note { get; set; }

        public decimal? ExtendedMaterialPrice => PricePerUnit * MaterialQuantity;

        public decimal? ExtendedServicePrice => FreightRate * FreightQuantity;

        public decimal? GrandTotal => (ExtendedMaterialPrice ?? 0) + (ExtendedServicePrice ?? 0);
    }
}
