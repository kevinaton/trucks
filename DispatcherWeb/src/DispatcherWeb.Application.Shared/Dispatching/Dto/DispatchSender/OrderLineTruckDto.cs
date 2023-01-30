﻿using System;
using Abp.Extensions;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class OrderLineTruckDto
    {
        public int OrderLineId { get; set; }
        public string TruckCode { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int TruckId { get; set; }
        public bool HasPhone => !DriverPhoneNumber.IsNullOrEmpty();
        public OrderNotifyPreferredFormat DriverPreferredNotifyFormat { get; set; }
        public int OrderLineTruckId { get; set; }
        public DateTime? TruckTimeOnJobUtc { get; set; }
        public bool IsDone { get; set; }
        public string DriverPhoneNumber { get; set; }
        public long? UserId { get; set; }
        public OrderLineDto OrderLine { get; set; }
    }
}
