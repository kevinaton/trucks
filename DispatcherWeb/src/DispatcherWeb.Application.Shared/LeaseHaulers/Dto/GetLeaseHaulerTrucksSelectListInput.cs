using DispatcherWeb.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulerTrucksSelectListInput : GetSelectListInput
    {
        public int LeaseHaulerId { get; set; }
    }
}
