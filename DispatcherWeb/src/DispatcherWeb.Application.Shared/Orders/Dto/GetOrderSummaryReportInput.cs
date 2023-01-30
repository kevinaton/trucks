using System;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderSummaryReportInput
    {
        public DateTime Date { get; set; }
        public bool ShowLoadColumns { get; set; }
        public bool HidePrices { get; set; }
    }
}
