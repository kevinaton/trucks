using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Zero.Configuration;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Permissions.Dto;
using DispatcherWeb.Authorization.Roles.Dto;
using System.Linq.Dynamic.Core;

namespace DispatcherWeb.Authorization.Roles
{
    /// <summary>
    /// Application service that is used by 'role management' page.
    /// </summary>
    [AbpAuthorize(AppPermissions.Pages_Administration_Roles)]
    public class RoleAppService : DispatcherWebAppServiceBase, IRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly IRoleManagementConfig _roleManagementConfig;

        public RoleAppService(
            RoleManager roleManager,
            IRoleManagementConfig roleManagementConfig)
        {
            _roleManager = roleManager;
            _roleManagementConfig = roleManagementConfig;
        }

        [HttpPost]
        public async Task<PagedResultDto<RoleListDto>> GetRoles(GetRolesInput input)
        {
            var query = GetRolesFilteredQuery(input);

            var roleCount = await query.CountAsync();

            var roles = query
                .OrderBy(input.Sorting)
                .PageBy(input);

            var rolesListDtos = await GetRoleListDtoList(roles);

            return new PagedResultDto<RoleListDto>(
                roleCount,
                rolesListDtos);
        }

        [HttpPost]
        public async Task<ListResultDto<RoleListDto>> GetRolesForDropdown()
        {
            var query = _roleManager.Roles;

            var rolesListDtos = await GetRoleListDtoList(query);

            return new ListResultDto<RoleListDto>(rolesListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Create, AppPermissions.Pages_Administration_Roles_Edit)]
        public async Task<GetRoleForEditOutput> GetRoleForEdit(NullableIdDto input)
        {
            var permissions = PermissionManager.GetAllPermissions();
            var grantedPermissions = new Permission[0];
            RoleEditDto roleEditDto;

            if (input.Id.HasValue) //Editing existing role?
            {
                var role = await _roleManager.GetRoleByIdAsync(input.Id.Value);
                grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
                roleEditDto = ObjectMapper.Map<RoleEditDto>(role);
            }
            else
            {
                roleEditDto = new RoleEditDto();
            }

            return new GetRoleForEditOutput
            {
                Role = roleEditDto,
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList(),
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
            };
        }

        public async Task CreateOrUpdateRole(CreateOrUpdateRoleInput input)
        {
            if (input.Role.Id.HasValue)
            {
                await UpdateRoleAsync(input);
            }
            else
            {
                await CreateRoleAsync(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Delete)]
        public async Task<bool> IsRoleHaveUsers(EntityDto input)
        {
            bool _retVal = false;
            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            var users = await UserManager.GetUsersInRoleAsync(role.Name);
            if (users.Count() > 0)
                _retVal = true;

            return _retVal;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Delete)]
        public async Task DeleteRole(EntityDto input)
        {
            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            var users = await UserManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in users)
            {
                CheckErrors(await UserManager.RemoveFromRoleAsync(user, role.Name));
            }

            CheckErrors(await _roleManager.DeleteAsync(role));
        }

        public async Task RestoreDefaultPermissionsAsync(int roleId)
        {
            Role role = await _roleManager.GetRoleByIdAsync(roleId);
            await _roleManager.RestoreDefaultPermissionsAsync(role);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Edit)]
        protected virtual async Task UpdateRoleAsync(CreateOrUpdateRoleInput input)
        {
            Debug.Assert(input.Role.Id != null, "input.Role.Id should be set.");

            var role = await _roleManager.GetRoleByIdAsync(input.Role.Id.Value);
            role.DisplayName = input.Role.DisplayName;
            role.IsDefault = input.Role.IsDefault;

            await UpdateGrantedPermissionsAsync(role, input.GrantedPermissionNames);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Create)]
        protected virtual async Task CreateRoleAsync(CreateOrUpdateRoleInput input)
        {
            var role = new Role(AbpSession.TenantId, input.Role.DisplayName) { IsDefault = input.Role.IsDefault };
            CheckErrors(await _roleManager.CreateAsync(role));
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the role.
            await UpdateGrantedPermissionsAsync(role, input.GrantedPermissionNames);
        }

        private async Task UpdateGrantedPermissionsAsync(Role role, List<string> grantedPermissionNames)
        {
            var grantedPermissions = PermissionManager.GetPermissionsFromNamesByValidating(grantedPermissionNames);
            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);
        }

        private static async Task<List<RoleListDto>> GetRoleListDtoList(IQueryable<Role> roles)
        {
            var data = await (from r in roles
                              select new RoleListDto
                              {
                                  Id = r.Id,
                                  Name = r.Name,
                                  DisplayName = r.DisplayName,
                                  IsStatic = r.IsStatic,
                                  IsDefault = r.IsDefault,
                                  CreationTime = r.CreationTime
                              }).ToListAsync();


            var roleListDtos = data;
            return roleListDtos;
        }

        private IQueryable<Role> GetRolesFilteredQuery(IGetRolesInput input)
        {
            var query = _roleManager.Roles;

            if (!string.IsNullOrEmpty(input.Permission))
            {
                var staticRoleNames = _roleManagementConfig.StaticRoles.Where(
                    r => r.GrantAllPermissionsByDefault &&
                         r.Side == AbpSession.MultiTenancySide
                ).Select(r => r.RoleName).ToList();

                query = query.Where(r =>
                    r.Permissions.Any(rp => rp.Name == input.Permission)
                        ? r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                        : staticRoleNames.Contains(r.Name)
                );
            }

            return query;
        }
    }
}
