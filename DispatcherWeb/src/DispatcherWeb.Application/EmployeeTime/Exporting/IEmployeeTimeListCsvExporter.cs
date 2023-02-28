using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;

namespace DispatcherWeb.EmployeeTime.Exporting
{
    public interface IEmployeeTimeListCsvExporter
    {
        FileDto ExportToFile(List<EmployeeTimeDto> employeeTimeDtos);
    }
}
