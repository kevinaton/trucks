using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverApp.Logs.Dto
{
    public class GetMinLevelToUploadInput
    {
        public long? UserId { get; set; }
        public Guid? DeviceGuid { get; set; }
    }
}
