using System;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class GetLeaseHaulerRequestForEditInput
    {
        public int? LeaseHaulerRequestId { get; set; }

        public DateTime? Date { get; set; }
    }
}
