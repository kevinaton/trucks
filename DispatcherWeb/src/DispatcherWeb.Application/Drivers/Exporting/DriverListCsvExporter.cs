using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    AddHeader(
                        "First Name",
                        "Last Name",
                        "Office Name",
                        "Inactive",
                        "License Number",
                        "Type of License",
                        "License Expiration Date",
                        "Last Physical Date",
                        "Next Physical Due Date",
                        "Last MVR Date",
                        "Next MVR Due Date",
                        "Date of Hire"
                    );

                    AddObjects(
                        driverDtos,
                        _ => _.FirstName,
                        _ => _.LastName,
                        _ => _.OfficeName,
                        _ => _.IsInactive.ToYesNoString(),
                        _ => _.LicenseNumber,
                        _ => _.TypeOfLicense,
                        _ => _.LicenseExpirationDate?.ToString("d"),
                        _ => _.LastPhysicalDate?.ToString("d"),
                        _ => _.NextPhysicalDueDate?.ToString("d"),
                        _ => _.LastMvrDate?.ToString("d"),
                        _ => _.NextMvrDueDate?.ToString("d"),
                        _ => _.DateOfHire?.ToString("d")
                    );

                }
            );
        }

    }
}
