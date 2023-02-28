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
                    AddHeader(
                        L("Name"),
                        L("Surname"),
                        L("UserName"),
                        L("Office"),
                        L("PhoneNumber"),
                        L("EmailAddress"),
                        L("EmailConfirm"),
                        L("Roles"),
                        L("LastLoginTime"),
                        L("Active"),
                        L("CreationTime")
                    );

                    AddObjects(
                        userListDtos,
                        _ => _.Name,
                        _ => _.Surname,
                        _ => _.UserName,
                        _ => _.OfficeName,
                        _ => _.PhoneNumber,
                        _ => _.EmailAddress,
                        _ => _.IsEmailConfirmed.ToYesNoString(),
                        _ => _.Roles.Select(r => r.RoleName).JoinAsString(", "),
                        _ => _timeZoneConverter.Convert(_.LastLoginTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToShortDateString(),
                        _ => _.IsActive.ToYesNoString(),
                        _ => _timeZoneConverter.Convert(_.CreationTime, _abpSession.TenantId, _abpSession.GetUserId())?.ToShortDateString()
                    );

                }
            );
        }

    }
}
