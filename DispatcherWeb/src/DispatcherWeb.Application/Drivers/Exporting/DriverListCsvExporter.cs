using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Drivers.Exporting
{
    public class DriverListCsvExporter : CsvExporterBase, IDriverListCsvExporter
    {
        public DriverListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<DriverDto> driverDtos)
        {
            return CreateCsvFile(
                "DriverList.csv",
                () =>
                {
                    AddHeaderAndData(
                        driverDtos,
                        ("First Name", x => x.FirstName),
                        ("Last Name", x => x.LastName),
                        ("Office Name", x => x.OfficeName),
                        ("Inactive", x => x.IsInactive.ToYesNoString()),
                        ("License Number", x => x.LicenseNumber),
                        ("License State", x => x.LicenseState),
                        ("Type of License", x => x.TypeOfLicense),
                        ("License Expiration Date", x => x.LicenseExpirationDate?.ToString("d")),
                        ("Last Physical Date", x => x.LastPhysicalDate?.ToString("d")),
                        ("Next Physical Due Date", x => x.NextPhysicalDueDate?.ToString("d")),
                        ("Last MVR Date", x => x.LastMvrDate?.ToString("d")),
                        ("Next MVR Due Date", x => x.NextMvrDueDate?.ToString("d")),
                        ("Date of Hire", x => x.DateOfHire?.ToString("d")),
                        ("Termination Date", x => x.TerminationDate?.ToString("d")),
                        ("Employee Id", x => x.EmployeeId)
                    );

                }
            );
        }

    }
}
