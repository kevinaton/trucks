using System.Collections.Generic;
using DispatcherWeb.Authorization.Users.Importing.Dto;
using Abp.Dependency;

namespace DispatcherWeb.Authorization.Users.Importing
{
    public interface IUserListExcelDataReader: ITransientDependency
    {
        List<ImportUserDto> GetUsersFromExcel(byte[] fileBytes);
    }
}
