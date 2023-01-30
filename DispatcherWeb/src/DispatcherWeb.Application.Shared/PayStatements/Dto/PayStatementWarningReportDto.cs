using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementWarningReportDto
    {
        public DateTime EndDate { get; set; }
        public List<PayStatementDriverDateConflictDto> DriverDateConflicts { get; set; }
    }
}
