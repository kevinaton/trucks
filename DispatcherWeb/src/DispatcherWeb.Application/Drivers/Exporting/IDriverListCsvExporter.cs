using System.Collections.Generic;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers.Exporting
{
    public interface IDriverListCsvExporter
    {
        FileDto ExportToFile(List<DriverDto> driverDtos);
    }
}