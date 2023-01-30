using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTruckUtilizationDataOutput
    {
        public GetTruckUtilizationDataOutput(List<TruckUtilizationData> truckUtilizationData)
        {
            TruckUtilizationData = truckUtilizationData;
        }
        public List<TruckUtilizationData> TruckUtilizationData { get; set; }
    }
}
