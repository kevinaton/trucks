using System.Collections.Generic;
using Abp.Collections.Extensions;
using Abp.Dependency;
using DispatcherWeb.Authorization.Users.Importing.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Authorization.Users.Importing
{
    public class InvalidUserExporter : CsvExporterBase, IInvalidUserExporter, ITransientDependency
    {
        public InvalidUserExporter(ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<ImportUserDto> userListDtos)
        {
            return CreateCsvFile(
                "InvalidUserImportList.csv",
                () =>
                {
                    AddHeader(
                        L("UserName"),
                        L("Name"),
                        L("Surname"),
                        L("EmailAddress"),
                        L("PhoneNumber"),
                        L("Password"),
                        L("Roles"),
                        L("Refuse Reason")
                    );

                    AddObjects(
                        userListDtos,
                        _ => _.UserName,
                        _ => _.Name,
                        _ => _.Surname,
                        _ => _.EmailAddress,
                        _ => _.PhoneNumber,
                        _ => _.Password,
                        _ => _.AssignedRoleNames?.JoinAsString(","),
                        _ => _.Exception
                    );
                });
        }
    }
}
