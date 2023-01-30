using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class DriverAppInfo
    {
        public bool IsDriver { get; set; }
        public bool IsAdmin { get; set; }
        public long UserId { get; set; }
        public GetElapsedTimeResult ElapsedTime { get; set; }
        public bool UseShifts { get; set; }
        public bool UseBackgroundSync { get; set; }
        public int HttpRequestTimeout { get; set; }
        public IDictionary<int, string> ShiftNames { get; set; }
        public Guid DriverGuid { get; set; }
        public string DriverName { get; set; }
        public int? DriverLeaseHaulerId { get; set; }
        public bool HideTicketControls { get; set; }
        public bool RequireToEnterTickets { get; set; }
        public bool RequireSignature { get; set; }
        public bool RequireTicketPhoto { get; set; }
        public string TextForSignatureView { get; set; }
        public bool DispatchesLockedToTruck { get; set; }
        public int? DeviceId { get; set; }
        public List<TimeClassificationDto> TimeClassifications { get; set; }
        public int ProductionPayId { get; set; }
    }
}
