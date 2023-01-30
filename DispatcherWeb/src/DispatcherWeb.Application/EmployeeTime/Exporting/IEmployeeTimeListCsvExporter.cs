using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EmployeeTime.Exporting
{
    public interface IEmployeeTimeListCsvExporter
    {
        FileDto ExportToFile(List<EmployeeTimeDto> employeeTimeDtos);
    }
}
