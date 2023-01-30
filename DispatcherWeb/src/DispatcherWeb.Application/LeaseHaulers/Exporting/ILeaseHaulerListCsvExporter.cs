using System.Collections.Generic;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers.Exporting
{
    public interface ILeaseHaulerListCsvExporter
    {
        FileDto ExportToFile(List<LeaseHaulerEditDto> leaseHaulerEditDtos);
    }
}