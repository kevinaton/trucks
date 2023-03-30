using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.EmployeeTime.Exporting
{
    public class EmployeeTimeListCsvExporter : CsvExporterBase, IEmployeeTimeListCsvExporter
    {
        public EmployeeTimeListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<EmployeeTimeDto> employeeTimeDtos)
        {
            return CreateCsvFile(
                "EmployeeTimeList.csv",
                () =>
                {
                    AddHeaderAndData(
                        employeeTimeDtos,
                        (L("Employee"), x => x.EmployeeName),
                        (L("StartDateTime"), x => x.StartDateTime.ToString("f")),
                        (L("EndDateTime"), x => x.EndDateTime?.ToString("f")),
                        (L("TimeClassification"), x => x.TimeClassificationName),
                        (L("ElapsedTimeHr"), x => x.ElapsedHours.ToString("N1"))
                    );

                }
            );
        }
    }
}
