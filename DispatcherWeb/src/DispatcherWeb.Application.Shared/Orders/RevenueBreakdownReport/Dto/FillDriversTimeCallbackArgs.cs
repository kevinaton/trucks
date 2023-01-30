using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class FillDriversTimeCallbackArgs
    {
        public int EmployeeTimeId { get; set; }
        public int? TruckId { get; set; }
        public long UserId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal HoursToAdd { get; set; }
        public int TimeClassificationId { get; set; }
        public bool IsProductionBased { get; set; }
    }
}
