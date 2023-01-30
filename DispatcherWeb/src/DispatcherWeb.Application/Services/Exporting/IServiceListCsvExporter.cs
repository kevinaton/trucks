using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.Services.Dto;

namespace DispatcherWeb.Services.Exporting
{
    public interface IServiceListCsvExporter
    {
        FileDto ExportToFile(List<ServiceDto> serviceDtos);
    }
}