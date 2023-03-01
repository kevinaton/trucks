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
                    AddHeader(
                        L("Time"),
                        L("UserName"),
                        L("Service"),
                        L("Action"),
                        L("Parameters"),
                        L("Duration"),
                        L("IpAddress"),
                        L("Client"),
                        L("Browser"),
                        L("ErrorState")
                    );

                    AddObjects(
                        auditLogListDtos,
                        _ => _timeZoneConverter.Convert(_.ExecutionTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                        _ => _.UserName,
                        _ => _.ServiceName,
                        _ => _.MethodName,
                        _ => _.Parameters,
                        _ => _.ExecutionDuration.ToString(),
                        _ => _.ClientIpAddress,
                        _ => _.ClientName,
                        _ => _.BrowserInfo,
                        _ => _.Exception.IsNullOrEmpty() ? L("Success") : _.Exception
                        );
                });
        }

        public FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos)
        {
            return CreateCsvFile(
                "DetailedLogs.xlsx",
                () =>
                {
                    AddHeader(
                        L("Action"),
                        L("Object"),
                        L("UserName"),
                        L("Time")
                    );

                    AddObjects(
                        entityChangeListDtos,
                        _ => _.ChangeType.ToString(),
                        _ => _.EntityTypeFullName,
                        _ => _.UserName,
                        _ => _timeZoneConverter.Convert(_.ChangeTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")
                    );
                });
        }
    }
}
