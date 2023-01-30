using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.PayStatements.Dto
{
    public class GetDriverPayStatementReportInput
    {
        public int Id { get; set; }
        public bool SplitByDriver { get; set; }
    }
}
