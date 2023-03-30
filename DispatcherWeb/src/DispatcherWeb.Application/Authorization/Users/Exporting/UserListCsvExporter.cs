using System.Collections.Generic;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Authorization.Users.Exporting
{
    public class UserListCsvExporter : CsvExporterBase, IUserListCsvExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public UserListCsvExporter(
            ITempFileCacheManager tempFileCacheManager,
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession
        ) : base(tempFileCacheManager)
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
                        (L("Office"), x => x.OfficeName),
                        (L("PhoneNumber"), x => x.PhoneNumber),
                        (L("EmailAddress"), x => x.EmailAddress),
                        (L("EmailConfirm"), x => x.IsEmailConfirmed.ToYesNoString()),
                        (L("Roles"), x => x.Roles.Select(r => r.RoleName).JoinAsString(", ")),
                        (L("LastLoginTime"), x => _timeZoneConverter.Convert(x.LastLoginTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToShortDateString()),
                        (L("Active"), x => x.IsActive.ToYesNoString()),
                        (L("CreationTime"), x => _timeZoneConverter.Convert(x.CreationTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToShortDateString())
                    );

                }
            );
        }

    }
}
