using Abp.Extensions;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverInfoBaseDto
    {

    }

    public class DriverInfoNotFoundDto : DriverInfoBaseDto
    {
        
    }

    public class DriverInfoDeletedDto : DriverInfoBaseDto
    {
        
    }

    public class DriverInfoErrorAndRedirect : DriverInfoBaseDto
    {
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
        public string UrlText { get; set; }
    }

    public abstract class DriverInfoDto : DriverInfoBaseDto
    {
        public int DispatchId { get; set; }
        public Guid Guid { get; set; }

        public string CustomerName { get; set; }
        public DispatchStatus DispatchStatus { get; set; }
        public bool IsMultipleLoads { get; set; }
        public bool WasMultipleLoads { get; set; }
        public int TenantId { get; set; }
    }

    public class DriverInfoCompletedDto : DriverInfoDto
    {

    }

    public class DriverInfoCanceledDto : DriverInfoDto
    {

    }

    public class DriverInfoExpiredDto : DriverInfoDto
    {

    }

    public class DriverLoadInfoDto : DriverInfoDto
    {
        public string Item { get; set; }
        public DesignationEnum Designation { get; set; }
        public string PickupAt => LoadAtName + ". " + LoadAt?.FormattedAddress;
        [JsonIgnore]
        public string LoadAtName { get; set; }
        [JsonIgnore]
        public LocationAddressDto LoadAt { get; set; }
        public string TicketNumber { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal Amount { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public string Note { get; set; }
        public string ChargeTo { get; set; }
        public int? LoadId { get; set; }
        public Guid? SignatureId { get; set; }
    }

    public class DriverDestinationInfoDto : DriverInfoDto
    {
        public string CustomerAddress => DeliverToName + ". " + DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public string DeliverToName { get; set; }
        [JsonIgnore]
        public LocationAddressDto DeliverTo { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public string Note { get; set; }
        public Guid? SignatureId { get; set; }
    }

    public class DriverLoadDestinationInfoDto : DriverInfoDto
    {
        public string Item { get; set; }
        public DesignationEnum Designation { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public string PickupAt => (!LoadAtName.IsNullOrEmpty() ? LoadAtName + ". " : null) + LoadAtAddress; //TODO: this was left for backward compatibility, the field can be removed after every driver application on every client device is updated
        public string LoadAtName { get; set; }
        public string LoadAtAddress => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationAddressDto LoadAt { get; set; }
        public decimal? LoadAtLatitude { get; set; }
        public decimal? LoadAtLongitude { get; set; }
        public string TicketNumber { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal MaterialAmount { get; set; }
        public decimal FreightAmount { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public string Note { get; set; }
        public string ChargeTo { get; set; }
        public int? LoadId { get; set; }
        public Guid? SignatureId { get; set; }
        public DateTime? Acknowledged { get; set; }
        public DateTime? Loaded { get; set; }

        public string CustomerAddress => (!DeliverToName.IsNullOrEmpty() ? DeliverToName + ". " : null) + DeliverToAddress; //TODO: this was left for backward compatibility, the field can be removed after every driver application on every client device is updated
        public string DeliverToName { get; set; }
        public string DeliverToAddress => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationAddressDto DeliverTo { get; set; }
        public decimal? DeliverToLatitude { get; set; }
        public decimal? DeliverToLongitude { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public int DispatchOrder => SortOrder; //backwards compatibility, remove later
        public int NumberOfLoadsToFinish { get; set; }
        public int NumberOfAddedLoads { get; set; }
        public bool ProductionPay { get; set; }
        public string TruckCode { get; set; }
        public bool HasTickets { get; set; }
    }
}
