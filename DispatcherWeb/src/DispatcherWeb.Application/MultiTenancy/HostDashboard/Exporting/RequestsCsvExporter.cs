using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    AddHeader(
                        "Request name",
                        "Ave. Exec. Time",
                        "# yesterday",
                        "# in last week",
                        "# in last month"
                    );

                    AddObjects(
                        requestDtos,
                        _ => $"{_.ServiceName}.{_.MethodName}",
                        _ => _.AverageExecutionDuration.ToString("N0"),
                        _ => _.NumberOfTransactions.ToString("N0"),
                        _ => _.LastWeekNumberOfTransactions.ToString("N0"),
                        _ => _.LastMonthNumberOfTransactions.ToString("N0")
                    );

                }
            );
        }

    }
}
