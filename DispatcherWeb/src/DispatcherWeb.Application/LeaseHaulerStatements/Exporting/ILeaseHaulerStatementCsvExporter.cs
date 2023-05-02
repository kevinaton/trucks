using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerStatements.Dto;

namespace DispatcherWeb.LeaseHaulerStatements.Exporting
{
    public interface ILeaseHaulerStatementCsvExporter : ICsvExporter
    {
        FileDto ExportToFile(LeaseHaulerStatementReportDto data);
        FileBytesDto ExportToFileBytes(LeaseHaulerStatementReportDto data);
    }
}
