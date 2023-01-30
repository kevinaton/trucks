using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class LeaseHaulerStatementReportDto
    {
        public int Id { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<LeaseHaulerStatementTicketReportDto> Tickets { get; set; }
    }
}
