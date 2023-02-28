using System;

namespace DispatcherWeb.DriverApp.Logs.Dto
{
    public class GetMinLevelToUploadInput
    {
        public long? UserId { get; set; }
        public Guid? DeviceGuid { get; set; }
    }
}
