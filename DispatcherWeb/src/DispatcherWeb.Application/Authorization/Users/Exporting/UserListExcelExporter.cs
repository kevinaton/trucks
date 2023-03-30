using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.DashboardCustomization.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Authorization.Users.Exporting
{
    public class UserListExcelExporter : CsvExporterBase, IUserListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public UserListExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<UserListDto> userListDtos)
        {
            return CreateCsvFile(
                "UserList.csv",
                () =>
                {
                    AddHeaderAndData(
                        userListDtos,
                        (L("Name"), x => x.Name),
                        (L("Surname"), x => x.Surname),
                        (L("UserName"), x => x.UserName),
                        (L("PhoneNumber"), x => x.PhoneNumber),
                        (L("EmailAddress"), x => x.EmailAddress),
                        (L("EmailConfirm"), x => x.IsEmailConfirmed.ToYesNoString()),
                        (L("Roles"), x => x.Roles.Select(r => r.RoleName).JoinAsString(", ")),
                        (L("LastLoginTime"), x => ConvertAndFormatDateTime(x.LastLoginTime)),
                        (L("Active"), x => x.IsActive.ToYesNoString()),
                        (L("CreationTime"), x => ConvertAndFormatDateTime(x.CreationTime))
                    );
                });
        }

        private string ConvertAndFormatDateTime(DateTime? value)
        {
            return _timeZoneConverter.Convert(value, _abpSession.TenantId, _abpSession.GetUserId())?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
        }
    }
}
