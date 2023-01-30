using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.Samsara
{
    public class VehiclesStatsResult
    {
        public List<VehicleStats> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
