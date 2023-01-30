using System.Collections.Generic;
using DispatcherWeb.Authorization.Users.Importing.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Authorization.Users.Importing
{
    public interface IInvalidUserExporter
    {
        FileDto ExportToFile(List<ImportUserDto> userListDtos);
    }
}
