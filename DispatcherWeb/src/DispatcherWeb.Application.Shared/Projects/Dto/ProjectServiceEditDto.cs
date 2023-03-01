using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Infrastructure;
using Newtonsoft.Json;

namespace DispatcherWeb.Projects.Dto
{
    public class ProjectServiceEditDto
    {
        public int? Id { get; set; }

        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtNamePlain { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToNamePlain { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }

        public decimal? PricePerUnit { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }
    }
}
