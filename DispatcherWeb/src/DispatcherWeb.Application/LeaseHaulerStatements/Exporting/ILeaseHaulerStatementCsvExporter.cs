using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerStatements.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.LeaseHaulerStatements.Exporting
{
    public interface ILeaseHaulerStatementCsvExporter
    {
        FileDto ExportToFile(LeaseHaulerStatementReportDto data);
    }
}
