using System;
using System.Collections.Generic;

namespace DispatcherWeb.DriverApp.Logs.Dto
{
    public class PostInput
    {
        public Guid? DeviceGuid { get; set; }    
        public List<LogDto> Logs { get; set; }
    }
}
