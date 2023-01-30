using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.Samsara
{
    public class VehicleStat<T>
    {
        public DateTime Time { get; set; }
        public T Value { get; set; }
    }
}
