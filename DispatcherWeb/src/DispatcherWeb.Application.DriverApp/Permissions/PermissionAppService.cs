using Abp.Application.Services.Dto;
using Abp.Authorization;

namespace DispatcherWeb.DriverApp.Permissions
{
    [AbpAuthorize]
    public class PermissionAppService : DispatcherWebDriverAppAppServiceBase, IPermissionAppService
    {
        public async Task<IListResult<string>> Get()
        {
            var user = await GetCurrentUserAsync();
            var grantedPermissions = await UserManager.GetGrantedPermissionsAsync(user);
            return new ListResultDto<string>(grantedPermissions.Select(x => x.Name).ToList());
        }
    }
}
