using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class GetRevenueGraphDataOutput
    {
        public GetRevenueGraphDataOutput(List<RevenueGraphData> revenueGraphData)
        {
            RevenueGraphData = revenueGraphData;
        }

        public List<RevenueGraphData> RevenueGraphData { get; set; }
    }
}
