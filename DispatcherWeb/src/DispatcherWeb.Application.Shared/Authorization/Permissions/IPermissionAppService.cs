using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Authorization.Permissions.Dto;

namespace DispatcherWeb.Authorization.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        ListResultDto<FlatPermissionWithLevelDto> GetAllPermissions();
    }
}
