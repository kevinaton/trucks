using System.Collections.Generic;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Auditing.Exporting
{
    public class AuditLogListExcelExporter : CsvExporterBase, IAuditLogListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public AuditLogListExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<AuditLogListDto> auditLogListDtos)
        {
            return CreateCsvFile(
                "AuditLogs.csv",
                () =>
                {
                    AddHeaderAndData(
                        auditLogListDtos,
                        (L("Time"), x => _timeZoneConverter.Convert(x.ExecutionTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")),
                        (L("UserName"), x => x.UserName),
                        (L("Service"), x => x.ServiceName),
                        (L("Action"), x => x.MethodName),
                        (L("Parameters"), x => x.Parameters),
                        (L("Duration"), x => x.ExecutionDuration.ToString()),
                        (L("IpAddress"), x => x.ClientIpAddress),
                        (L("Client"), x => x.ClientName),
                        (L("Browser"), x => x.BrowserInfo),
                        (L("ErrorState"), x => x.Exception.IsNullOrEmpty() ? L("Success") : x.Exception)
                    );
                });
        }

        public FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos)
        {
            return CreateCsvFile(
                "DetailedLogs.xlsx",
                () =>
                {
                    AddHeaderAndData(
                        entityChangeListDtos,
                        (L("Action"), x => x.ChangeType.ToString()),
                        (L("Object"), x => x.EntityTypeFullName),
                        (L("UserName"), x => x.UserName),
                        (L("Time"), x => _timeZoneConverter.Convert(x.ChangeTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"))
                    );
                });
        }
    }
}
