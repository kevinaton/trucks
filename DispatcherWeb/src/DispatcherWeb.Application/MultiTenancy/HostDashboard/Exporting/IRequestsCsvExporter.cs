using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Exporting
{
    public interface IRequestsCsvExporter
    {
        FileDto ExportToFile(List<RequestDto> dispatchDtos);
    }
}