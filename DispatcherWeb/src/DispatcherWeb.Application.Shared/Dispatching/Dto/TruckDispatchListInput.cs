using System.Collections.Generic;

namespace DispatcherWeb.Dispatching.Dto
{
    public class TruckDispatchListInput
    {
        public int? OfficeId { get; set; }
        public DispatchListViewEnum View { get; set; }
        public List<int> DispatchIds { get; set; }
        public List<int> DriverIds { get; set; }
        public List<int> TruckIds { get; set; }
    }
}
