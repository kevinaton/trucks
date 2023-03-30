using System.Collections.Generic;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Customers.Exporting
{
    public class CustomerListCsvExporter : CsvExporterBase, ICustomerListCsvExporter
    {
        public CustomerListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<CustomerDto> customerDtos)
        {
            return CreateCsvFile(
                "CustomerList.csv",
                () =>
                {
                    AddHeaderAndData(
                        customerDtos,
                        ("Name", x => x.Name),
                        ("Account", x => x.AccountNumber),
                        ("Address 1", x => x.Address1),
                        ("Address 2", x => x.Address2),
                        ("City", x => x.City),
                        ("State", x => x.State),
                        ("Zip Code", x => x.ZipCode),
                        ("Country Code", x => x.CountryCode),
                        ("Active", x => x.IsActive.ToYesNoString())
                    );
                }
            );
        }

    }
}
