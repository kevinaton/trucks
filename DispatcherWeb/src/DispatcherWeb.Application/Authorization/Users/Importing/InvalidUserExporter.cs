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
                    AddHeaderAndData(
                        userListDtos,
                        (L("UserName"), x => x.UserName),
                        (L("Name"), x => x.Name),
                        (L("Surname"), x => x.Surname),
                        (L("EmailAddress"), x => x.EmailAddress),
                        (L("PhoneNumber"), x => x.PhoneNumber),
                        (L("Password"), x => x.Password),
                        (L("Roles"), x => x.AssignedRoleNames?.JoinAsString(",")),
                        (L("Refuse Reason"), x => x.Exception)
                    );
                });
        }
    }
}
