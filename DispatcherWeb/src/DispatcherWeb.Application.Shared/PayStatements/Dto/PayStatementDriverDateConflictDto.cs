using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementDriverDateConflictDto
    {
        public DateTime Date { get; set; }
        public string DriverName { get; set; }
        public DriverDateConflictKind ConflictKind { get; set; }
    }
}
