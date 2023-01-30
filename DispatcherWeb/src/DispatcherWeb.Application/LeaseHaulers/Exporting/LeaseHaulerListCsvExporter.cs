using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.LeaseHaulers.Exporting
{
    public class LeaseHaulerListCsvExporter : CsvExporterBase, ILeaseHaulerListCsvExporter
    {
        public LeaseHaulerListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<LeaseHaulerEditDto> leaseHaulerEditDtos)
        {
            return CreateCsvFile(
                "LeaseHaulerList.csv",
                () =>
                {
                    AddHeader(
                        "Name",
                        "Street Address 1",
                        "Street Address 2",
                        "City",
                        "State",
                        "Zip Code",
                        "Country Code",
                        "Account Number",
                        "Phone Number"
                    );

                    AddObjects(
                        leaseHaulerEditDtos,
                        _ => _.Name,
                        _ => _.StreetAddress1,
                        _ => _.StreetAddress2,
                        _ => _.City,
                        _ => _.State,
                        _ => _.ZipCode,
                        _ => _.CountryCode,
                        _ => _.AccountNumber,
                        _ => _.PhoneNumber
                    );

                }
            );
        }

    }
}
