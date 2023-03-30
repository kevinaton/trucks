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
                    AddHeaderAndData(
                        timeOffDtos,
                        (L("Driver"), x => x.DriverName),
                        (L("StartDate"), x => x.StartDate.ToString("f")),
                        (L("EndDate"), x => x.EndDate.ToString("f")),
                        (L("Reason"), x => x.Reason),
                        (L("RequestedHrs"), x => x.RequestedHours?.ToString("N1"))
                    );
                }
            );
        }
    }
}
