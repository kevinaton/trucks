using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Storage;
using DispatcherWeb.TimeOffs.Dto;

namespace DispatcherWeb.TimeOffs.Exporting
{
    public class TimeOffListCsvExporter : CsvExporterBase, ITimeOffListCsvExporter
    {
        public TimeOffListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<TimeOffDto> timeOffDtos)
        {
            return CreateCsvFile(
                "TimeOffList.csv",
                () =>
                {
                    AddHeader(
                        L("Driver"),
                        L("StartDate"),
                        L("EndDate"),
                        L("Reason"),
                        L("RequestedHrs")
                    );

                    AddObjects(
                        timeOffDtos,
                        _ => _.DriverName,
                        _ => _.StartDate.ToString("f"),
                        _ => _.EndDate.ToString("f"),
                        _ => _.Reason,
                        _ => _.RequestedHours?.ToString("N1")
                    );

                }
            );
        }
    }
}
