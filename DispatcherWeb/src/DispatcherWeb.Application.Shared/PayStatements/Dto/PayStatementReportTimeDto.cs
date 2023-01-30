using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportTimeDto
    {
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public decimal? DriverPayRate { get; set; }
        public string TimeClassificationName { get; set; }
        public int TimeClassificationId { get; set; }
        public bool IsProductionPay { get; set; }
    }
}
