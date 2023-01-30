using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Telematics.Dto
{
    public class TruckCurrentData
    {
        public string TruckCodeOrUniqueId { get; set; }
        public double CurrentMileage { get; set; }
        public double CurrentHours { get; set; }
    }
}
