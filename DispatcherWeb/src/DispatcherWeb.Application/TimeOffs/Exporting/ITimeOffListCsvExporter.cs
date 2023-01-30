using DispatcherWeb.Dto;
using DispatcherWeb.TimeOffs.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.TimeOffs.Exporting
{
    public interface ITimeOffListCsvExporter
    {
        FileDto ExportToFile(List<TimeOffDto> timeOffDtos);
    }
}
