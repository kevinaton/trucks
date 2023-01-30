using System.Collections.Generic;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Exporting
{
    public interface ICustomerListCsvExporter
    {
        FileDto ExportToFile(List<CustomerDto> customerDtos);
    }
}