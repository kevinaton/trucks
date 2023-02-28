using System;
using System.Collections.Generic;
using Abp.Extensions;
using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Dispatching.Dto
{
    public class TruckDispatchListItemDto
    {
        public TruckDispatchListItemDto()
        {
            Dispatches = new List<TruckDispatch>();
        }

        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? DriverId { get; set; }
        public long? UserId { get; set; }
        public bool IsClockedIn { get; set; }
        public bool HasClockedInToday { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FullName => Utilities.FormatFullName(FirstName, LastName);
        public List<TruckDispatch> Dispatches { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public bool IsExternal { get; set; }
        public DateTime? StartTime { get; set; }

        public class TruckDispatch
        {
            public int Id { get; set; }
            public int SortOrder { get; set; }
            public Guid Guid { get; set; }
            public string ShortGuid { get; set; }
            public DispatchStatus Status { get; set; }
            public string StatusName => Status.GetDisplayName();
            public DateTime? StartTime { get; set; }
            public string DeliverToName => DeliverTo?.FormattedAddress;
            [JsonIgnore]
            public LocationNameDto DeliverTo { get; set; }
            public DateTime DeliveryDate { get; set; }
            public Shift? Shift { get; set; }
            public DateTime? TimeOnJob { get; set; }
            public string CustomerName { get; set; }
            public string LoadAtName => LoadAt?.FormattedAddress;
            [JsonIgnore]
            public LocationNameDto LoadAt { get; set; }
            public string Item { get; set; }
            public string MaterialUom { get; set; }
            public string FreightUom { get; set; }
            public DateTime Created { get; set; }
            public DateTime? Acknowledged { get; set; }
            public DateTime? Sent { get; set; }
            public DateTime? Loaded { get; set; }
            public DateTime? Complete { get; set; }
            public bool IsMultipleLoads { get; set; }
            public bool WasMultipleLoads { get; set; }

            public bool IsDraggable => Status.IsIn(DispatchStatus.Created, DispatchStatus.Sent);

            public string StatusTime
            {
                get
                {
                    DateTime? time = null;
                    switch (Status)
                    {
                        case DispatchStatus.Created:
                        case DispatchStatus.Error:
                            time = Created;
                            break;
                        case DispatchStatus.Sent:
                            time = Sent;
                            break;
                        case DispatchStatus.Acknowledged:
                            time = Acknowledged;
                            break;
                        case DispatchStatus.Loaded:
                            time = Loaded;
                            break;
                        case DispatchStatus.Completed:
                            time = Complete;
                            break;
                    }
                    return $"{time:t}";
                }
            }
        }


    }
}
