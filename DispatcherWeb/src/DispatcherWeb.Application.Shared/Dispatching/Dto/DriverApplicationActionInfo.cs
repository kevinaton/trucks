using DispatcherWeb.DriverApplication.Dto;
using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverApplicationActionInfo
    {
        public DriverApplicationActionInfo()
        {
        }

        public DriverApplicationActionInfo(ExecuteDriverApplicationActionInput input, AuthDriverByDriverGuidResult authInfo)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ActionTime = input.ActionTime; //we'll remove the field once we're sure everyone has updated to the latest version
#pragma warning restore CS0618 // Type or member is obsolete
            if (input.ActionTimeInUtc.HasValue)
            {
                ActionTimeInUtc = input.ActionTimeInUtc.Value;
            }
            DeviceId = input.DeviceId;
            DeviceGuid = input.DeviceGuid;
            TenantId = authInfo.TenantId;
            DriverId = authInfo.DriverId;
            UserId = authInfo.UserId;
        }

        public int TenantId { get; set; }
        public int DriverId { get; set; }
        public long UserId { get; set; }
        public int? DeviceId { get; set; }
        public Guid? DeviceGuid { get; set; }
        
        [Obsolete("Use ActionTimeInUtc instead")]
        public DateTime? ActionTime { get; set; }

        public DateTime ActionTimeInUtc { get; set; }
        public string TimeZone { get; set; }
    }
}
