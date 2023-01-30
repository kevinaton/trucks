using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Localization;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using DispatcherWeb.Authorization.Users;
using System;

namespace DispatcherWeb.Authorization.Roles
{
    /// <summary>
    /// Role manager.
    /// Used to implement domain logic for roles.
    /// </summary>
    public class RoleManager : AbpRoleManager<Role, User>
    {
        private readonly IPermissionManager _permissionManager;
        private readonly ILocalizationManager _localizationManager;

        public RoleManager(
            RoleStore store,
            IEnumerable<IRoleValidator<Role>> roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<RoleManager> logger,
            IPermissionManager permissionManager,
            IRoleManagementConfig roleManagementConfig,
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            ILocalizationManager localizationManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository)
            : base(
                store,
                roleValidators,
                keyNormalizer,
                errors,
                logger,
                permissionManager,
                cacheManager,
                unitOfWorkManager,
                roleManagementConfig,
                organizationUnitRepository,
                organizationUnitRoleRepository)
        {
            _localizationManager = localizationManager;
            _permissionManager = permissionManager;
        }

        public override Task SetGrantedPermissionsAsync(Role role, IEnumerable<Permission> permissions)
        {
            CheckPermissionsToUpdate(role, permissions);

            return base.SetGrantedPermissionsAsync(role, permissions);
        }

        public virtual async Task<Role> GetRoleByIdAsync(long roleId)
        {
            var role = await FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                throw new ApplicationException("There is no role with id: " + roleId);
            }

            return role;
        }

        private void CheckPermissionsToUpdate(Role role, IEnumerable<Permission> permissions)
        {
            if (role.Name == StaticRoleNames.Host.Admin &&
                (!permissions.Any(p => p.Name == AppPermissions.Pages_Administration_Roles_Edit) ||
                 !permissions.Any(p => p.Name == AppPermissions.Pages_Administration_Users_ChangePermissions)))
            {
                throw new UserFriendlyException(L("YouCannotRemoveUserRolePermissionsFromAdminRole"));
            }
        }

        private new string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        public async Task RestoreDefaultPermissionsAsync(Role role)
        {
            foreach(string permissionName in DefaultRolePermissions.DefaultPermissions)
            {
                if(DefaultRolePermissions.IsPermissionsGrantedToRole(role.Name, permissionName))
                {
                    await GrantPermissionToRoleAsync(role, permissionName);
                }
                else
                {
                    await RevokePermissionFromRoleAsync(role, permissionName);
                }
            }
        }
        private async Task GrantPermissionToRoleAsync(Role role, string permissionName)
        {
            var permission = _permissionManager.GetPermission(permissionName);
            await GrantPermissionAsync(role, permission);
        }

        private async Task RevokePermissionFromRoleAsync(Role role, string permissionName)
        {
            var permission = _permissionManager.GetPermission(permissionName);
            await ProhibitPermissionAsync(role, permission);
        }

    }
}
