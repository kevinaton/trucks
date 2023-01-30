using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks.Exporting
{
    public interface ITruckListCsvExporter
    {
        FileDto ExportToFile(List<TruckEditDto> truckEditDtos);
    }
}