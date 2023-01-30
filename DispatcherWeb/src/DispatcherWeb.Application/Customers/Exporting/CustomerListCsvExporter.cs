using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dispatching.Dto;
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
                    AddHeader(
                        "Name",
                        "Account",
                        "Address 1",
                        "Address 2",
                        "City",
                        "State",
                        "Zip Code",
                        "Country Code",
                        "Active"
                    );

                    AddObjects(
                        customerDtos,
                        _ => _.Name,
                        _ => _.AccountNumber,
                        _ => _.Address1,
                        _ => _.Address2,
                        _ => _.City,
                        _ => _.State,
                        _ => _.ZipCode,
                        _ => _.CountryCode,
                        _ => _.IsActive.ToYesNoString()
                    );

                }
            );
        }

    }
}
