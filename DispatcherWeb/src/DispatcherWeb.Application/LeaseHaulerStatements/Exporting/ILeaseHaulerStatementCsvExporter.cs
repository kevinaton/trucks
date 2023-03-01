using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerStatements.Dto;

namespace DispatcherWeb.LeaseHaulerStatements.Exporting
{
    public interface ILeaseHaulerStatementCsvExporter
    {
        FileDto ExportToFile(LeaseHaulerStatementReportDto data);
    }
}
