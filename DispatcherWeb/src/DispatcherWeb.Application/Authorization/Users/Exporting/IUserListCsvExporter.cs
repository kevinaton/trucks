using System.Collections.Generic;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Authorization.Users.Exporting
{
    public interface IUserListCsvExporter
    {
        FileDto ExportToFile(List<UserListDto> userListDtos);
    }
}