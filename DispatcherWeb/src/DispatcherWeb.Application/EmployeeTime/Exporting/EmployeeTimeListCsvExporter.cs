using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;
using DispatcherWeb.Storage;
using System;
using System.Collections.Generic;
using System.Text;

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
                    AddHeader(
                        L("Employee"),
                        L("StartDateTime"),
                        L("EndDateTime"),
                        L("TimeClassification"),
                        L("ElapsedTimeHr")
                    );

                    AddObjects(
                        employeeTimeDtos,
                        _ => _.EmployeeName,
                        _ => _.StartDateTime.ToString("f"),
                        _ => _.EndDateTime?.ToString("f"),
                        _ => _.TimeClassificationName,
                        _ => _.ElapsedHours.ToString("N1")
                    );

                }
            );
        }
    }
}
