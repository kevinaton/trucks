using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Permissions.Dto;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Authorization.Users.Exporting;
using DispatcherWeb.Chat;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Friendships;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Organizations.Dto;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Users)]
    public class UserAppService : DispatcherWebAppServiceBase, IUserAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly RoleManager _roleManager;
        private readonly IUserEmailer _userEmailer;
        private readonly IUserListExcelExporter _userListExcelExporter;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<RolePermissionSetting, long> _rolePermissionRepository;
        private readonly IRepository<UserPermissionSetting, long> _userPermissionRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUserPolicy _userPolicy;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRoleManagementConfig _roleManagementConfig;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Quotes.Quote> _quoteRepository;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly ICustomerContactUserLinkService _customerContactUserLinkService;
        private readonly IUserListCsvExporter _userListCsvExporter;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IUserCreatorService _userCreatorService;

        public UserAppService(
            RoleManager roleManager,
            IUserEmailer userEmailer,
            IUserListExcelExporter userListExcelExporter,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IRepository<RolePermissionSetting, long> rolePermissionRepository,
            IRepository<UserPermissionSetting, long> userPermissionRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role> roleRepository,
            IUserPolicy userPolicy,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            IPasswordHasher<User> passwordHasher,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRoleManagementConfig roleManagementConfig,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Quotes.Quote> quoteRepository,
            IRepository<Friendship, long> friendshipRepository,
            IRepository<ChatMessage, long> chatMessageRepository,
            IDriverUserLinkService driverUserLinkService,
            ICustomerContactUserLinkService customerContactUserLinkService,
            IUserListCsvExporter userListCsvExporter,
            ISingleOfficeAppService singleOfficeService,
            IUserCreatorService userCreatorService
            )
        {
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _userListExcelExporter = userListExcelExporter;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _rolePermissionRepository = rolePermissionRepository;
            _userPermissionRepository = userPermissionRepository;
            _userRoleRepository = userRoleRepository;
            _userPolicy = userPolicy;
            _passwordValidators = passwordValidators;
            _passwordHasher = passwordHasher;
            _organizationUnitRepository = organizationUnitRepository;
            _roleManagementConfig = roleManagementConfig;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
            _roleRepository = roleRepository;
            AppUrlService = NullAppUrlService.Instance;
            _dispatchRepository = dispatchRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _driverRepository = driverRepository;
            _quoteRepository = quoteRepository;
            _friendshipRepository = friendshipRepository;
            _chatMessageRepository = chatMessageRepository;
            _driverUserLinkService = driverUserLinkService;
            _customerContactUserLinkService = customerContactUserLinkService;
            _userListCsvExporter = userListCsvExporter;
            _singleOfficeService = singleOfficeService;
            _userCreatorService = userCreatorService;
        }

        [HttpPost]
        public async Task<PagedResultDto<UserListDto>> GetUsers(GetUsersInput input)
        {
            var query = GetUsersFilteredQuery(input);

            var userCount = await query.CountAsync();

            var users = ToUserListDto(query)
                .OrderBy(input.Sorting)
                .PageBy(input);

            var userListDtos = await GetUserListDtoList(users);
            await FillRoleNames(userListDtos);

            return new PagedResultDto<UserListDto>(
                userCount,
                userListDtos
            );
        }

        public async Task<FileDto> GetUsersToExcel(GetUsersToExcelInput input)
        {
            var query = GetUsersFilteredQuery(input);

            var users = await query
                .OrderBy(input.Sorting)
                .ToListAsync();

            var userListDtos = ObjectMapper.Map<List<UserListDto>>(users);
            await FillRoleNames(userListDtos);

            return _userListExcelExporter.ExportToFile(userListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create, AppPermissions.Pages_Administration_Users_Edit)]
        public async Task<GetUserForEditOutput> GetUserForEdit(NullableIdDto<long> input)
        {
            //Getting all available roles
            var userRoleDtos = await _roleManager.Roles
                .OrderBy(r => r.DisplayName)
                .Select(r => new UserRoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleDisplayName = r.DisplayName
                })
                .ToArrayAsync();

            var allOrganizationUnits = await _organizationUnitRepository.GetAllListAsync();

            var output = new GetUserForEditOutput
            {
                Roles = userRoleDtos,
                AllOrganizationUnits = ObjectMapper.Map<List<OrganizationUnitDto>>(allOrganizationUnits),
                MemberedOrganizationUnits = new List<string>()
            };

            if (!input.Id.HasValue)
            {
                //Creating a new user
                output.User = new UserEditDto
                {
                    IsActive = true,
                    ShouldChangePasswordOnNextLogin = true,
                    IsTwoFactorEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                            .TwoFactorLogin.IsEnabled),
                    IsLockoutEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut
                            .IsEnabled)
                };

                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    var defaultUserRole = userRoleDtos.FirstOrDefault(ur => ur.RoleName == defaultRole.Name);
                    if (defaultUserRole != null)
                    {
                        defaultUserRole.IsAssigned = true;
                    }
                }
            }
            else
            {
                //Editing an existing user
                var user = await UserManager.Users.Include(x => x.Office).FirstOrDefaultAsync(x => x.Id == input.Id.Value);

                output.User = ObjectMapper.Map<UserEditDto>(user);
                output.ProfilePictureId = user.ProfilePictureId;                

                var organizationUnits = await UserManager.GetOrganizationUnitsAsync(user);
                output.MemberedOrganizationUnits = organizationUnits.Select(ou => ou.Code).ToList();

                var allRolesOfUsersOrganizationUnits = GetAllRoleNamesOfUsersOrganizationUnits(input.Id.Value);

                foreach (var userRoleDto in userRoleDtos)
                {
                    userRoleDto.IsAssigned = await UserManager.IsInRoleAsync(user, userRoleDto.RoleName);
                    userRoleDto.InheritedFromOrganizationUnit =
                        allRolesOfUsersOrganizationUnits.Contains(userRoleDto.RoleName);
                }
            }

            var dispatchViaDriverApp = await SettingManager.DispatchViaDriverApplication();
            var allowLeaseHaulers = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit);
            output.Roles = output.Roles.Where(x =>
            {
                switch (x.RoleName)
                {
                    case StaticRoleNames.Tenants.Driver:
                        return x.IsAssigned || dispatchViaDriverApp;
                    case StaticRoleNames.Tenants.LeaseHaulerDriver:
                        return x.IsAssigned || dispatchViaDriverApp && allowLeaseHaulers;
                    default:
                        return true;
                }
            }).ToArray();

            await _singleOfficeService.FillSingleOffice(output.User);

            return output;
        }

        private List<string> GetAllRoleNamesOfUsersOrganizationUnits(long userId)
        {
            return (from userOu in _userOrganizationUnitRepository.GetAll()
                    join roleOu in _organizationUnitRoleRepository.GetAll() on userOu.OrganizationUnitId equals roleOu
                        .OrganizationUnitId
                    join userOuRoles in _roleRepository.GetAll() on roleOu.RoleId equals userOuRoles.Id
                    where userOu.UserId == userId
                    select userOuRoles.Name).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task<GetUserPermissionsForEditOutput> GetUserPermissionsForEdit(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            var permissions = PermissionManager.GetAllPermissions();
            var grantedPermissions = await UserManager.GetGrantedPermissionsAsync(user);

            return new GetUserPermissionsForEditOutput
            {
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName)
                    .ToList(),
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task ResetUserSpecificPermissions(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            await UserManager.ResetAllPermissionsAsync(user);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task UpdateUserPermissions(UpdateUserPermissionsInput input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            var grantedPermissions =
                PermissionManager.GetPermissionsFromNamesByValidating(input.GrantedPermissionNames);
            await UserManager.SetGrantedPermissionsAsync(user, grantedPermissions);
        }

        public async Task CreateOrUpdateUser(CreateOrUpdateUserInput input)
        {
            if (input.User.Id.HasValue)
            {
                await UpdateUserAsync(input);
            }
            else
            {
                await CreateUserAsync(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Delete)]
        public async Task DeleteUser(EntityDto<long> input)
        {
            if (input.Id == AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("YouCanNotDeleteOwnAccount"));
            }

            var user = await UserManager.GetUserByIdAsync(input.Id);
            if (await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver))
            {
                if (await HasLeaseHaulerRequests(user.Id))
                {
                    throw new UserFriendlyException(L("UnableToDeleteLhDriverWithRequests"));
                }
                var drivers = await _driverRepository.GetAll()
                    .Where(x => x.UserId == user.Id)
                    .ToListAsync();
                drivers.ForEach(x => x.UserId = null);
            }

            if (await _quoteRepository.GetAll().AnyAsync(x => x.SalesPersonId == input.Id))
            {
                throw new UserFriendlyException(L("UnableToDeleteUserWithAssociatedData"));
            }

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                if (await _friendshipRepository.GetAll().AnyAsync(x => x.UserId == input.Id || x.FriendUserId == input.Id)
                    || await _chatMessageRepository.GetAll().AnyAsync(x => x.UserId == input.Id || x.TargetUserId == input.Id))
                {
                    throw new UserFriendlyException(L("UnableToDeleteUserWithAssociatedData"));
                }
            }

            await _driverUserLinkService.EnsureCanDeleteUser(user);
            CheckErrors(await UserManager.DeleteAsync(user));
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Unlock)]
        public async Task UnlockUser(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            user.Unlock();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Edit)]
        protected virtual async Task UpdateUserAsync(CreateOrUpdateUserInput input)
        {
            Debug.Assert(input.User.Id != null, "input.User.Id should be set.");

            var user = await UserManager.FindByIdAsync(input.User.Id.Value.ToString());
            if (user.IsActive && !input.User.IsActive)
            {
                if (await HasOpenDispatchesAsync(user.Id))
                {
                    throw new UserFriendlyException(L("CantDeactivateUserBecauseOfDispatches"));
                }
            }

            //Update user properties
            user.Name = input.User.Name;
            user.Surname = input.User.Surname;
            user.UserName = input.User.UserName;
            user.EmailAddress = input.User.EmailAddress;
            user.PhoneNumber = input.User.PhoneNumber;
            user.Office = null;
            user.OfficeId = input.User.OfficeId;
            user.IsActive = input.User.IsActive;
            user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;
            user.IsTwoFactorEnabled = input.User.IsTwoFactorEnabled;
            user.IsLockoutEnabled = input.User.IsLockoutEnabled;

            CheckErrors(await UserManager.UpdateAsync(user));

            if (input.SetRandomPassword)
            {
                var randomPassword = await UserManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.User.Password));
            }

            CheckErrors(await UserManager.UpdateAsync(user));

            //Update roles
            var currentUser = await UserManager.GetUserByIdAsync(AbpSession.GetUserId());
            if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Tenants.Admin))
            {
                //if current user is not Admin, do not allow to add or remove Admin role
                var hasAdminRole = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin);
                if (hasAdminRole && !input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Admin))
                {
                    input.AssignedRoleNames = input.AssignedRoleNames.Union(new[] { StaticRoleNames.Tenants.Admin }).ToArray();
                }
                else if (!hasAdminRole && input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Admin))
                {
                    input.AssignedRoleNames = input.AssignedRoleNames.Except(new[] { StaticRoleNames.Tenants.Admin }).ToArray();
                }
            }
            if (await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Driver)
                && !input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Driver))
            {
                if (await HasOpenDispatchesAsync(user.Id))
                {
                    throw new UserFriendlyException(L("CantRemoveDriverRoleBecauseOfDispatches"));
                }
            }
            CheckErrors(await UserManager.SetRolesAsync(user, input.AssignedRoleNames));

            await _driverUserLinkService.UpdateDriver(user);
            await _customerContactUserLinkService.UpdateCustomerContact(user);

            //update organization units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            if (input.SendActivationEmail)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    input.User.Password
                );
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create)]
        protected virtual async Task CreateUserAsync(CreateOrUpdateUserInput input)
        {
            var user = await _userCreatorService.CreateUser(input);

            if (AbpSession.TenantId.HasValue)
            {
                user.TenantId = AbpSession.TenantId.Value;
            }
            await _driverUserLinkService.UpdateDriver(user);

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task FillRoleNames(IReadOnlyCollection<UserListDto> userListDtos)
        {
            /* This method is optimized to fill role names to given list. */
            var userIds = userListDtos.Select(u => u.Id);

            var userRoles = await _userRoleRepository.GetAll()
                .Where(userRole => userIds.Contains(userRole.UserId))
                .Select(userRole => userRole).ToListAsync();

            var distinctRoleIds = userRoles.Select(userRole => userRole.RoleId).Distinct();

            foreach (var user in userListDtos)
            {
                var rolesOfUser = userRoles.Where(userRole => userRole.UserId == user.Id).ToList();
                user.Roles = ObjectMapper.Map<List<UserListRoleDto>>(rolesOfUser);
            }

            var roleNames = new Dictionary<int, string>();
            foreach (var roleId in distinctRoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role != null)
                {
                    roleNames[roleId] = role.DisplayName;
                }
            }

            foreach (var userListDto in userListDtos)
            {
                foreach (var userListRoleDto in userListDto.Roles)
                {
                    if (roleNames.ContainsKey(userListRoleDto.RoleId))
                    {
                        userListRoleDto.RoleName = roleNames[userListRoleDto.RoleId];
                    }
                }

                userListDto.Roles = userListDto.Roles.Where(r => r.RoleName != null).OrderBy(r => r.RoleName).ToList();
            }
        }

        private IQueryable<User> GetUsersFilteredQuery(IGetUsersInput input)
        {
            var query = UserManager.Users
                .Include(u => u.Office)
                .WhereIf(input.Role.HasValue, u => u.Roles.Any(r => r.RoleId == input.Role.Value))
                .WhereIf(input.OnlyLockedUsers,
                    u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTime.UtcNow)
                .WhereIf(
                    !input.Filter.IsNullOrWhiteSpace(),
                    u =>
                        u.Name.Contains(input.Filter) ||
                        u.Surname.Contains(input.Filter) ||
                        u.UserName.Contains(input.Filter) ||
                        u.EmailAddress.Contains(input.Filter)
                )
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId);

            if (input.Permissions != null && input.Permissions.Any(p => !p.IsNullOrWhiteSpace()))
            {
                var staticRoleNames = _roleManagementConfig.StaticRoles.Where(
                    r => r.GrantAllPermissionsByDefault &&
                         r.Side == AbpSession.MultiTenancySide
                ).Select(r => r.RoleName).ToList();

                input.Permissions = input.Permissions.Where(p => !string.IsNullOrEmpty(p)).ToList();

                var userIds = from user in query
                              join ur in _userRoleRepository.GetAll() on user.Id equals ur.UserId into urJoined
                              from ur in urJoined.DefaultIfEmpty()
                              join urr in _roleRepository.GetAll() on ur.RoleId equals urr.Id into urrJoined
                              from urr in urrJoined.DefaultIfEmpty()
                              join up in _userPermissionRepository.GetAll()
                                  .Where(userPermission => input.Permissions.Contains(userPermission.Name)) on user.Id equals up.UserId into upJoined
                              from up in upJoined.DefaultIfEmpty()
                              join rp in _rolePermissionRepository.GetAll()
                                  .Where(rolePermission => input.Permissions.Contains(rolePermission.Name)) on
                                  new { RoleId = ur == null ? 0 : ur.RoleId } equals new { rp.RoleId } into rpJoined
                              from rp in rpJoined.DefaultIfEmpty()
                              where (up != null && up.IsGranted) ||
                                    (up == null && rp != null && rp.IsGranted) ||
                                    (up == null && rp == null && staticRoleNames.Contains(urr.Name))
                              group user by user.Id
                    into userGrouped
                              select userGrouped.Key;

                query = UserManager.Users.Where(e => userIds.Contains(e.Id));
            }

            return query;
        }

        private IQueryable<UserListDto> ToUserListDto(IQueryable<User> users)
        {
            return users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Surname = u.Surname,
                    UserName = u.UserName,
                    EmailAddress = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    ProfilePictureId = u.ProfilePictureId,
                    IsEmailConfirmed = u.IsEmailConfirmed,
                    OfficeName = u.Office.Name,
                    LastLoginTime = u.LastLoginTime,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLockoutEnabled,
                    CreationTime = u.CreationTime
                });
        }

        private async Task<List<UserListDto>> GetUserListDtoList(IQueryable<UserListDto> users)
        {
            var userListDtos = await users.ToListAsync();

            await FillRoleNames(userListDtos);
            return userListDtos;
        }

        [HttpPost]
        public async Task<FileDto> GetUsersToCsv(GetUsersInput input)
        {
            var query = GetUsersFilteredQuery(input);
            var users = ToUserListDto(query)
                .OrderBy(input.Sorting);

            var items = await GetUserListDtoList(users);

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return _userListCsvExporter.ExportToFile(items);
        }

        private async Task<bool> HasOpenDispatchesAsync(long userId)
        {
            return await _dispatchRepository.GetAll()
                .AnyAsync(x => x.Driver.UserId == userId && !Dispatch.ClosedDispatchStatuses.Contains(x.Status));
        }

        private async Task<bool> HasLeaseHaulerRequests(long userId)
        {
            return await _availableLeaseHaulerTruckRepository.GetAll()
                .AnyAsync(x => x.Driver.UserId == userId);
        }
        public async Task<PagedResultDto<SelectListDto>> GetMaintenanceUsersSelectList(GetSelectListInput input)
        {
            int[] maintenanceRolesIdArray = await _roleManager.Roles
                .Where(r => r.Name == StaticRoleNames.Tenants.Maintenance || r.Name == StaticRoleNames.Tenants.MaintenanceSupervisor)
                .Select(r => r.Id)
                .ToArrayAsync();

            return await UserManager.Users
                    .Where(u => u.Roles.Any(r => maintenanceRolesIdArray.Contains(r.RoleId)))
                    .Select(u => new SelectListDto
                    {
                        Id = u.Id.ToString(),
                        Name = u.Name + " " + u.Surname
                    })
                    .GetSelectListResult(input)
                ;

        }

        public async Task<PagedResultDto<SelectListDto>> GetUsersSelectList(GetSelectListInput input)
        {
            var query = UserManager.Users
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name + " " + x.Surname
                });

            return await query.GetSelectListResult(input);
        }

        public async Task<PagedResultDto<SelectListDto>> GetSalespersonsSelectList(GetSelectListInput input)
        {
            var allowedRoles = await _roleManager.Roles
                .Where(x => x.Name != StaticRoleNames.Tenants.Driver && x.Name != StaticRoleNames.Tenants.LeaseHaulerDriver)
                .Select(x => x.Id)
                .ToListAsync();

            var query = UserManager.Users
                .Where(x => x.IsActive && x.Roles.Any(r => allowedRoles.Contains(r.RoleId)))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name + " " + x.Surname
                });

            return await query.GetSelectListResult(input);
        }

    }
}