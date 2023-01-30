using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard.Dto
{
    public class TruckUtilizationData
    {
        public TruckUtilizationData(int utilizationPercent, string period)
        {
            UtilizationPercent = utilizationPercent;
            Period = period;
        }

        public int UtilizationPercent { get; set; }
        public string Period { get; set; }
    }
}
