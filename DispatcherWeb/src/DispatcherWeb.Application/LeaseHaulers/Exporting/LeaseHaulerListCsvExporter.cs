using System.Collections.Generic;
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
                    AddHeaderAndData(
                        leaseHaulerEditDtos,
                        ("Name", x => x.Name),
                        ("Street Address 1", x => x.StreetAddress1),
                        ("Street Address 2", x => x.StreetAddress2),
                        ("City", x => x.City),
                        ("State", x => x.State),
                        ("Zip Code", x => x.ZipCode),
                        ("Country Code", x => x.CountryCode),
                        ("Account Number", x => x.AccountNumber),
                        ("Phone Number", x => x.PhoneNumber)
                    );
                }
            );
        }

    }
}
