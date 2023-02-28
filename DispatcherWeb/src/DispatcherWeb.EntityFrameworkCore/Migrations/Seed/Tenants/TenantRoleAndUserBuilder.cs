using System.Linq;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class TenantRoleAndUserBuilder
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public TenantRoleAndUserBuilder(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            //Admin role

            var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null)
            {
                adminRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true }).Entity;
                _context.SaveChanges();

                //Grant all permissions to admin role
                var permissions = PermissionFinder
                    .GetAllPermissions(new AppAuthorizationProvider(false))
                    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant))
                    .Where(p => !AppPermissions.ManualPermissionsList.Contains(p.Name))
                    .ToList();

                foreach (var permission in permissions)
                {
                    GrantPermission(adminRole.Id, permission.Name);
                }
                _context.SaveChanges();
            }

            //cleanup
            var allRoleIds = _context.Roles.IgnoreQueryFilters().Where(r => r.TenantId == _tenantId).Select(x => x.Id).ToList();
            var oldUserRoles = _context.UserRoles.IgnoreQueryFilters().Where(r => r.TenantId == _tenantId).Where(x => !allRoleIds.Contains(x.RoleId)).ToList();
            oldUserRoles.ForEach(x => _context.UserRoles.Remove(x));

            //User role

            var userRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.User);
            if (userRole != null && userRole.IsDeleted)
            {
                userRole.IsDeleted = false;
                userRole.DeleterUserId = null;
                userRole.DeletionTime = null;
                userRole.IsDefault = true;
                userRole.IsStatic = true;
                _context.SaveChanges();
            }
            if (userRole == null)
            {
                userRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.User, StaticRoleNames.Tenants.User) { IsStatic = true }).Entity;
                userRole.IsDefault = true;
                _context.SaveChanges();

                GrantPermission(userRole.Id, DefaultRolePermissions.GetRolePermissions(StaticRoleNames.Tenants.User).ToArray());
                _context.SaveChanges();
            }


            //Dispatching role

            var dispatchingRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Dispatching);
            if (dispatchingRole == null)
            {
                dispatchingRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Dispatching, StaticRoleNames.Tenants.Dispatching) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(dispatchingRole.Id, new[]
                {
                    AppPermissions.Pages_Dashboard,
                    AppPermissions.Pages_Orders_View,
                    AppPermissions.Pages_Orders_Edit,
                    AppPermissions.Pages_Schedule,
                    AppPermissions.Pages_PrintOrders,
                    AppPermissions.Pages_Dispatches,
                    AppPermissions.Pages_Dispatches_Edit,
                    AppPermissions.Pages_DriverAssignment,
                    AppPermissions.Pages_LeaseHauler,
                    AppPermissions.Pages_LeaseHaulers_Edit,
                    AppPermissions.Pages_Trucks,
                    AppPermissions.Pages_Customers,
                    AppPermissions.Pages_Services,
                    AppPermissions.Pages_Drivers,
                    AppPermissions.Pages_Locations
                });
                _context.SaveChanges();
            }

            //LimitedQuoting role

            var limitedQuotingRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.LimitedQuoting);
            if (limitedQuotingRole == null)
            {
                limitedQuotingRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.LimitedQuoting, StaticRoleNames.Tenants.LimitedQuoting) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(limitedQuotingRole.Id, new[]
                {
                    AppPermissions.Pages_Quotes_View,
                    AppPermissions.Pages_Quotes_Items_Create
                });
                _context.SaveChanges();
            }

            //Quoting role

            var quotingRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Quoting);
            if (quotingRole == null)
            {
                quotingRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Quoting, StaticRoleNames.Tenants.Quoting) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(quotingRole.Id, new[]
                {
                    AppPermissions.Pages_Dashboard,
                    AppPermissions.Pages_Projects,
                    AppPermissions.Pages_Quotes_View,
                    AppPermissions.Pages_Quotes_Edit,
                    AppPermissions.Pages_Quotes_Items_Create
                });
                _context.SaveChanges();
            }

            //Backoffice role

            var backofficeRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Backoffice);
            if (backofficeRole == null)
            {
                backofficeRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Backoffice, StaticRoleNames.Tenants.Backoffice) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(backofficeRole.Id, new[]
                {
                    AppPermissions.Pages_Dashboard,
                    AppPermissions.Pages_Orders_View,
                    AppPermissions.Pages_LeaseHauler,
                    AppPermissions.Pages_LeaseHaulers_Edit,
                    AppPermissions.Pages_TimeOff,
                    AppPermissions.Pages_TimeEntry,
                    AppPermissions.Pages_TimeEntry_EditAll,
                    AppPermissions.Pages_TimeEntry_EditTimeClassifications,
                    AppPermissions.Pages_Backoffice_DriverPay
                });
                _context.SaveChanges();
            }

            //Driver role

            var driverRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Driver);
            if (driverRole == null)
            {
                driverRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Driver, StaticRoleNames.Tenants.Driver) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(driverRole.Id, new[]
                    {
                        AppPermissions.Pages_DriverApplication,
                    }
                );
                _context.SaveChanges();
            }

            //Lease Hauler Driver role

            var leaseHaulerDriverRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.LeaseHaulerDriver);
            if (leaseHaulerDriverRole == null)
            {
                leaseHaulerDriverRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.LeaseHaulerDriver, StaticRoleDisplayNames.Tenants.LeaseHaulerDriver) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(leaseHaulerDriverRole.Id, new[]
                    {
                        AppPermissions.Pages_DriverApplication,
                    }
                );
                _context.SaveChanges();
            }

            //Maintenance role

            var maintenanceRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Maintenance);
            if (maintenanceRole == null)
            {
                maintenanceRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Maintenance, StaticRoleNames.Tenants.Maintenance) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(maintenanceRole.Id, new[]
                {
                    AppPermissions.Pages_VehicleService_View,
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
                    AppPermissions.Pages_WorkOrders_View,
                    AppPermissions.Pages_WorkOrders_EditLimited,
                });
                _context.SaveChanges();
            }

            //MaintenanceSupervisor role

            var maintenanceSupervisorRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.MaintenanceSupervisor);
            if (maintenanceSupervisorRole == null)
            {
                maintenanceSupervisorRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.MaintenanceSupervisor, StaticRoleNames.Tenants.MaintenanceSupervisor) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(maintenanceSupervisorRole.Id, new[]
                {
                    AppPermissions.Pages_VehicleServiceTypes_View,
                    AppPermissions.Pages_VehicleServiceTypes_Edit,
                    AppPermissions.Pages_VehicleService_View,
                    AppPermissions.Pages_VehicleService_Edit,
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit,
                    AppPermissions.Pages_WorkOrders_View,
                    AppPermissions.Pages_WorkOrders_Edit,
                    AppPermissions.Pages_WorkOrders_EditLimited,
                });
                _context.SaveChanges();
            }

            //Administrative role

            var administrativeRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Administrative);
            if (administrativeRole == null)
            {
                administrativeRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Administrative, StaticRoleNames.Tenants.Administrative) { IsStatic = true }).Entity;
                _context.SaveChanges();


                GrantPermission(administrativeRole.Id, DefaultRolePermissions.GetRolePermissions(StaticRoleNames.Tenants.Administrative).ToArray());
                _context.SaveChanges();
            }

            //Managers role

            var managersRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Managers);
            if (managersRole == null)
            {
                managersRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Managers, StaticRoleNames.Tenants.Managers) { IsStatic = true }).Entity;
                _context.SaveChanges();

                GrantPermission(managersRole.Id, new[]
                {
                    AppPermissions.Pages_Dashboard,
                    AppPermissions.Pages_Orders_View,
                    AppPermissions.Pages_Orders_Edit,
                    AppPermissions.Pages_Schedule,
                    AppPermissions.Pages_PrintOrders,
                    AppPermissions.Pages_Dispatches,
                    AppPermissions.Pages_Dispatches_Edit,
                    AppPermissions.Pages_DriverAssignment,
                    AppPermissions.Pages_Trucks,
                    AppPermissions.Pages_Customers,
                    AppPermissions.Pages_Services,
                    AppPermissions.Pages_Drivers,
                    AppPermissions.Pages_Locations,
                    AppPermissions.Pages_Projects,
                    AppPermissions.Pages_Quotes_View,
                    AppPermissions.Pages_Quotes_Edit,
                    AppPermissions.Pages_Quotes_Items_Create,
                    AppPermissions.Pages_TimeOff,
                    AppPermissions.Pages_TimeEntry,
                    AppPermissions.Pages_TimeEntry_EditAll,
                    AppPermissions.Pages_TimeEntry_EditTimeClassifications
                });
                _context.SaveChanges();
            }

            //admin user

            var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == User.AdminUserName);
            if (adminUser == null)
            {
                adminUser = User.CreateTenantAdminUser(_tenantId, User.AdminUserName, User.AdminUserName, "admin@defaulttenant.com");
                adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "123qwe");
                adminUser.IsEmailConfirmed = true;
                adminUser.ShouldChangePasswordOnNextLogin = true;
                adminUser.IsActive = true;
                adminUser.OfficeId = _context.Offices.IgnoreQueryFilters().Where(o => o.TenantId == _tenantId).Select(x => (int?)x.Id).FirstOrDefault();

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                //Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();

                //User account of admin user
                if (_tenantId == 1)
                {
                    _context.UserAccounts.Add(new UserAccount
                    {
                        TenantId = _tenantId,
                        UserId = adminUser.Id,
                        UserName = User.AdminUserName,
                        EmailAddress = adminUser.EmailAddress
                    });
                    _context.SaveChanges();
                }
            }

            //Grant new permissions for existing roles

            GrantPermissionIfMissing(adminRole.Id, new[]
            {
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_Orders_View,
                AppPermissions.Pages_Orders_Edit,
                AppPermissions.Pages_Schedule,
                AppPermissions.Pages_PrintOrders,
                AppPermissions.Pages_DriverAssignment,
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_Trucks,
                AppPermissions.Pages_Customers,
                AppPermissions.Pages_Customers_Merge,
                AppPermissions.Pages_Services,
                AppPermissions.Pages_Services_Merge,
                AppPermissions.Pages_Drivers,
                AppPermissions.Pages_Locations,
                AppPermissions.Pages_Locations_Merge,
                AppPermissions.Pages_Projects,
                AppPermissions.Pages_Quotes_View,
                AppPermissions.Pages_Quotes_Edit,
                AppPermissions.Pages_Quotes_Items_Create,
                AppPermissions.Pages_CannedText,
                //should stay revoked, in case it's not a multioffice tenant
                //AppPermissions.Pages_Offices
            });

            GrantPermissionIfMissing(dispatchingRole.Id, new[]
            {
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_Dispatches,
                AppPermissions.Pages_Dispatches_Edit,
            });

            GrantPermissionIfMissing(managersRole.Id, new[]
            {
                AppPermissions.Pages_Dispatches,
                AppPermissions.Pages_Dispatches_Edit,
                AppPermissions.Pages_TimeOff,
                AppPermissions.Pages_TimeEntry,
                AppPermissions.Pages_TimeEntry_EditAll,
                AppPermissions.Pages_TimeEntry_EditTimeClassifications
            });

            GrantPermissionIfMissing(backofficeRole.Id, new[]
            {
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_TimeOff,
                AppPermissions.Pages_TimeEntry,
                AppPermissions.Pages_TimeEntry_EditAll,
                AppPermissions.Pages_TimeEntry_EditTimeClassifications,
                AppPermissions.Pages_Backoffice_DriverPay
            });

            GrantPermissionIfMissing(administrativeRole.Id, DefaultRolePermissions.GetRolePermissions(StaticRoleNames.Tenants.Administrative).ToArray());

            _context.SaveChanges();

        }

        private void GrantPermissionIfMissing(int roleId, string[] permissionNames)
        {
            var existingPermissions = _context.Permissions.IgnoreQueryFilters().OfType<RolePermissionSetting>()
                .Where(x => permissionNames.Contains(x.Name) && x.RoleId == roleId)
                .Select(x => x.Name).ToList();
            foreach (var permissionName in permissionNames.Except(existingPermissions).ToList())
            {
                GrantPermission(roleId, permissionName);
            }
        }

        private void GrantPermissionIfMissing(int roleId, string permissionName)
        {
            if (!_context.Permissions.OfType<RolePermissionSetting>().Any(x => x.Name == permissionName && x.RoleId == roleId))
            {
                GrantPermission(roleId, permissionName);
            }
        }

        private void GrantPermission(int roleId, string[] permissionNames)
        {
            foreach (var permissionName in permissionNames)
            {
                GrantPermission(roleId, permissionName);
            }
        }

        private void GrantPermission(int roleId, string permissionName)
        {
            _context.Permissions.Add(
                    new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permissionName,
                        IsGranted = true,
                        RoleId = roleId
                    });
        }
    }
}
