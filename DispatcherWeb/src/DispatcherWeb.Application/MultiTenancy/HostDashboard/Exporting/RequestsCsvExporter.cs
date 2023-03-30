using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Exporting
{
    public class RequestsCsvExporter : CsvExporterBase, IRequestsCsvExporter
    {
        public RequestsCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<RequestDto> requestDtos)
        {
            return CreateCsvFile(
                "Requests.csv",
                () =>
                {
                    AddHeaderAndData(
                        requestDtos,
                        ("Request name", x => $"{x.ServiceName}.{x.MethodName}"),
                        ("Ave. Exec. Time", x => x.AverageExecutionDuration.ToString("N0")),
                        ("# yesterday", x => x.NumberOfTransactions.ToString("N0")),
                        ("# in last week", x => x.LastWeekNumberOfTransactions.ToString("N0")),
                        ("# in last month", x => x.LastMonthNumberOfTransactions.ToString("N0"))
                    );
                }
            );
        }

    }
}
