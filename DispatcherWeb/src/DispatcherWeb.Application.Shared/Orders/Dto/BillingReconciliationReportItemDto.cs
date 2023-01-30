using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Orders.Dto
{
    public class BillingReconciliationReportItemDto : BillingReconciliationItemDto
    {
        public string Name { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();
    }
}