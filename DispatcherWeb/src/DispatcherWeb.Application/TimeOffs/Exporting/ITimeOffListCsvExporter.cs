using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.TimeOffs.Dto;

namespace DispatcherWeb.TimeOffs.Exporting
{
    public interface ITimeOffListCsvExporter
    {
        FileDto ExportToFile(List<TimeOffDto> timeOffDtos);
    }
}
