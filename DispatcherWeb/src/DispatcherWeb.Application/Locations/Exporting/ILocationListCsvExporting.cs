using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.Locations.Dto;

namespace DispatcherWeb.Locations.Exporting
{
    public interface ILocationListCsvExporting
    {
        FileDto ExportToFile(List<LocationDto> locationDtos);
    }
}