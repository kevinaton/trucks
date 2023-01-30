using System.Collections.Generic;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Auditing.Exporting
{
    public interface IAuditLogListExcelExporter
    {
        FileDto ExportToFile(List<AuditLogListDto> auditLogListDtos);

        FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
    }
}
