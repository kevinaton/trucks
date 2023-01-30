using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Abp;
using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.MultiTenancy;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Editions;
using DispatcherWeb.MultiTenancy.Demo;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Runtime.Security;
using Microsoft.AspNetCore.Identity;
using DispatcherWeb.Notifications;
using System;
using System.Diagnostics;
using Abp.BackgroundJobs;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Offices;
using DispatcherWeb.Features;
using DispatcherWeb.Authorization;
using Abp.Authorization;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using System.Text.RegularExpressions;

namespace DispatcherWeb.MultiTenancy
{
    /// <summary>
    /// Tenant manager.
    /// </summary>
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public IAbpSession AbpSession { get; set; }

        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IUserEmailer _userEmailer;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<SubscribableEdition> _subscribableEditionRepository;
        protected readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<Office> _officeRepository;
        private readonly IPermissionManager _permissionManager;
        private readonly IFeatureChecker _featureChecker;
        private readonly ISettingManager _settingManager;

        public TenantManager(
            IRepository<Tenant> tenantRepository,
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository,
            EditionManager editionManager,
            IUnitOfWorkManager unitOfWorkManager,
            RoleManager roleManager,
            IUserEmailer userEmailer,
            UserManager userManager,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IAbpZeroFeatureValueStore featureValueStore,
            IAbpZeroDbMigrator abpZeroDbMigrator,
            IPasswordHasher<User> passwordHasher,
            IRepository<SubscribableEdition> subscribableEditionRepository,
            IBackgroundJobManager backgroundJobManager,
            IRepository<Office> officeRepository,
            IPermissionManager permissionManager,
            IFeatureChecker featureChecker,
            ISettingManager settingManager
        ) : base(
            tenantRepository,
            tenantFeatureRepository,
            editionManager,
            featureValueStore
        )
        {
            AbpSession = NullAbpSession.Instance;

            _unitOfWorkManager = unitOfWorkManager;
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _userManager = userManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _abpZeroDbMigrator = abpZeroDbMigrator;
            _passwordHasher = passwordHasher;
            _subscribableEditionRepository = subscribableEditionRepository;
            _backgroundJobManager = backgroundJobManager;
            _officeRepository = officeRepository;
            _permissionManager = permissionManager;
            _featureChecker = featureChecker;
            _settingManager = settingManager;
        }

        private async Task<string> GetUniqueTenancyName(string companyName)
        {
            var tenancyNameBase = string.Join("", companyName.Trim().Replace(' ', '-').Where(x => char.IsLetterOrDigit(x) || x == '-'));
            int? tenancyNameSuffix = null;

            do
            {
                var tenancyName = tenancyNameBase + tenancyNameSuffix;
                if (!await Tenants.AnyAsync(x => x.TenancyName == tenancyName))
                {
                    return tenancyName;
                }
                tenancyNameSuffix = tenancyNameSuffix == null ? 2 : tenancyNameSuffix.Value + 1;
            }
            while (true);
        }

        public async Task<int> CreateWithAdminUserAsync(
            string companyName,
            string adminFirstName,
            string adminLastName,
            string adminPassword,
            string adminEmailAddress,
            string connectionString,
            bool isActive,
            int? editionId,
            bool shouldChangePasswordOnNextLogin,
            bool sendActivationEmail,
            DateTime? subscriptionEndDate,
            bool isInTrialPeriod,
            Func<string, string> getActivationUrlFromTenancyName)
        {
            int newTenantId;
            long newAdminId;

            await CheckEditionAsync(editionId, isInTrialPeriod);

            if (isInTrialPeriod && !subscriptionEndDate.HasValue)
            {
                throw new UserFriendlyException(LocalizationManager.GetString(
                    DispatcherWebConsts.LocalizationSourceName, "TrialWithoutEndDateErrorMessage"));
            }

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                var tenancyName = await GetUniqueTenancyName(companyName);

                //Create tenant
                var tenant = new Tenant(tenancyName, companyName)
                {
                    IsActive = isActive,
                    EditionId = editionId,
                    SubscriptionEndDateUtc = subscriptionEndDate?.ToUniversalTime(),
                    IsInTrialPeriod = isInTrialPeriod,
                    ConnectionString = connectionString.IsNullOrWhiteSpace()
                        ? null
                        : SimpleStringCipher.Instance.Encrypt(connectionString)
                };

                await CreateAsync(tenant);
                await _unitOfWorkManager.Current.SaveChangesAsync(); //To get new tenant's id.

                //Create tenant database
                _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

                //We are working entities of new tenant, so changing tenant filter
                using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
                {
                    await _settingManager.ChangeSettingForTenantAsync(tenant.Id, AppSettings.General.CompanyName, companyName);

                    //Create static roles for new tenant
                    CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));
                    await _unitOfWorkManager.Current.SaveChangesAsync(); //To get static role ids

                    //grant all permissions to admin role
                    var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                    await _roleManager.GrantAllPermissionsAsync(adminRole);
                    foreach (var permissionName in AppPermissions.ManualPermissionsList)
                    {
                        await _roleManager.ProhibitPermissionAsync(adminRole, _permissionManager.GetPermission(permissionName));
                    }

                    //User role should be default
                    var userRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.User);
                    userRole.IsDefault = true;
                    CheckErrors(await _roleManager.UpdateAsync(userRole));

                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Administrative);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Backoffice);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Driver);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.LeaseHaulerDriver);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Dispatching);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Quoting);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.LimitedQuoting);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Maintenance);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.MaintenanceSupervisor);
                    await GrantDefaultPermissionsToStaticRole(StaticRoleNames.Tenants.Managers);
                    //Create admin user for the tenant
                    var adminUser = User.CreateTenantAdminUser(tenant.Id, adminFirstName, adminLastName, adminEmailAddress);
                    adminUser.ShouldChangePasswordOnNextLogin = shouldChangePasswordOnNextLogin;
                    adminUser.IsActive = true;

                    if (adminPassword.IsNullOrEmpty())
                    {
                        adminPassword = await _userManager.CreateRandomPassword();
                    }
                    else
                    {
                        await _userManager.InitializeOptionsAsync(AbpSession.TenantId);
                        foreach (var validator in _userManager.PasswordValidators)
                        {
                            CheckErrors(await validator.ValidateAsync(_userManager, adminUser, adminPassword));
                        }
                    }

                    adminUser.Password = _passwordHasher.HashPassword(adminUser, adminPassword);

                    CheckErrors(await _userManager.CreateAsync(adminUser));
                    await _unitOfWorkManager.Current.SaveChangesAsync(); //To get admin user's id

                    await CreateDefaultOffice(adminUser);
                    //Assign admin user to admin role!
                    CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));

                    //Notifications
                    await _appNotifier.WelcomeToTheApplicationAsync(adminUser);

                    //Send activation email
                    if (sendActivationEmail)
                    {
                        adminUser.SetNewEmailConfirmationCode();
                        var emailActivationLink = getActivationUrlFromTenancyName(tenancyName);
                        await _userEmailer.SendEmailActivationLinkAsync(adminUser, emailActivationLink, adminPassword);
                    }

                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    await _backgroundJobManager.EnqueueAsync<TenantDemoDataBuilderJob, int>(tenant.Id);

                    newTenantId = tenant.Id;
                    newAdminId = adminUser.Id;
                }

                await uow.CompleteAsync();
            }

            //Used a second UOW since UOW above sets some permissions and _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync needs these permissions to be saved.
            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (_unitOfWorkManager.Current.SetTenantId(newTenantId))
                {
                    await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(
                        new UserIdentifier(newTenantId, newAdminId));
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
            }

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (_unitOfWorkManager.Current.SetTenantId(newTenantId))
                {
                    await RevokeOfficePermissionForSingleOfficeEditions(newTenantId, newAdminId);
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
            }

            return newTenantId;
        }

        protected override Task ValidateTenancyNameAsync(string tenancyName)
        {
            ValidateTenancyName(tenancyName);
            return Task.CompletedTask;
        }

        protected override void ValidateTenancyName(string tenancyName)
        {
            if (!Regex.IsMatch(tenancyName, "^[a-zA-Z0-9][a-zA-Z0-9_-]{1,}$"))
            {
                throw new UserFriendlyException(L("InvalidTenancyName"));
            }
        }

        private async Task RevokeOfficePermissionForSingleOfficeEditions(int tenantId, long adminUserId)
        {
            using (AbpSession.Use(tenantId, adminUserId))
            {
                if (await _featureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature))
                {
                    return;
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var officePermission = _permissionManager.GetPermission(AppPermissions.Pages_Offices);
            foreach (var role in roles)
            {
                await _roleManager.ProhibitPermissionAsync(role, officePermission);
            }
        }

        private async Task CreateDefaultOffice(User adminUser)
        {
            const string name = "Main";
            const string truckColor = "#e6e3e4";

            Office office = new Office()
            {
                Name = name,
                TruckColor = truckColor,
                DefaultStartTime = new DateTime(2000, 1, 1, 12, 0, 0) //12pm UTC / 7am EST
            };
            office.Users.Add(adminUser);
            await _officeRepository.InsertAsync(office);
        }

        public async Task CheckEditionAsync(int? editionId, bool isInTrialPeriod)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                if (!editionId.HasValue || !isInTrialPeriod)
                {
                    return;
                }

                var edition = await _subscribableEditionRepository.GetAsync(editionId.Value);
                if (!edition.IsFree)
                {
                    return;
                }

                var error = LocalizationManager.GetSource(DispatcherWebConsts.LocalizationSourceName)
                    .GetString("FreeEditionsCannotHaveTrialVersions");
                throw new UserFriendlyException(error);
            });
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public decimal GetUpgradePrice(SubscribableEdition currentEdition, SubscribableEdition targetEdition,
            int totalRemainingHourCount, PaymentPeriodType paymentPeriodType)
        {
            int numberOfHoursPerDay = 24;

            var totalRemainingDayCount = totalRemainingHourCount / numberOfHoursPerDay;
            var unusedPeriodCount = totalRemainingDayCount / (int)paymentPeriodType;
            var unusedHoursCount = totalRemainingHourCount % ((int)paymentPeriodType * numberOfHoursPerDay);

            decimal currentEditionPriceForUnusedPeriod = 0;
            decimal targetEditionPriceForUnusedPeriod = 0;

            var currentEditionPrice = currentEdition.GetPaymentAmount(paymentPeriodType);
            var targetEditionPrice = targetEdition.GetPaymentAmount(paymentPeriodType);

            if (currentEditionPrice > 0)
            {
                currentEditionPriceForUnusedPeriod = currentEditionPrice * unusedPeriodCount;
                currentEditionPriceForUnusedPeriod += (currentEditionPrice / (int)paymentPeriodType) /
                    numberOfHoursPerDay * unusedHoursCount;
            }

            if (targetEditionPrice > 0)
            {
                targetEditionPriceForUnusedPeriod = targetEditionPrice * unusedPeriodCount;
                targetEditionPriceForUnusedPeriod += (targetEditionPrice / (int)paymentPeriodType) /
                    numberOfHoursPerDay * unusedHoursCount;
            }

            return targetEditionPriceForUnusedPeriod - currentEditionPriceForUnusedPeriod;
        }

        public async Task<Tenant> UpdateTenantAsync(int tenantId, bool isActive, bool? isInTrialPeriod,
            PaymentPeriodType? paymentPeriodType, int editionId, EditionPaymentType editionPaymentType)
        {
            var tenant = await FindByIdAsync(tenantId);

            tenant.IsActive = isActive;

            if (isInTrialPeriod.HasValue)
            {
                tenant.IsInTrialPeriod = isInTrialPeriod.Value;
            }

            tenant.EditionId = editionId;

            if (paymentPeriodType.HasValue)
            {
                tenant.UpdateSubscriptionDateForPayment(paymentPeriodType.Value, editionPaymentType);
            }

            return tenant;
        }

        public async Task<EndSubscriptionResult> EndSubscriptionAsync(Tenant tenant, SubscribableEdition edition,
            DateTime nowUtc)
        {
            if (tenant.EditionId == null || tenant.HasUnlimitedTimeSubscription())
            {
                throw new Exception(
                    $"Can not end tenant {tenant.TenancyName} subscription for {edition.DisplayName} tenant has unlimited time subscription!");
            }

            Debug.Assert(tenant.SubscriptionEndDateUtc != null, "tenant.SubscriptionEndDateUtc != null");

            var subscriptionEndDateUtc = tenant.SubscriptionEndDateUtc.Value;
            if (!tenant.IsInTrialPeriod)
            {
                subscriptionEndDateUtc =
                    tenant.SubscriptionEndDateUtc.Value.AddDays(edition.WaitingDayAfterExpire ?? 0);
            }

            if (subscriptionEndDateUtc >= nowUtc)
            {
                throw new Exception(
                    $"Can not end tenant {tenant.TenancyName} subscription for {edition.DisplayName} since subscription has not expired yet!");
            }

            if (!tenant.IsInTrialPeriod && edition.ExpiringEditionId.HasValue)
            {
                tenant.EditionId = edition.ExpiringEditionId.Value;
                tenant.SubscriptionEndDateUtc = null;

                await UpdateAsync(tenant);

                return EndSubscriptionResult.AssignedToAnotherEdition;
            }

            tenant.IsActive = false;
            tenant.IsInTrialPeriod = false;

            await UpdateAsync(tenant);

            return EndSubscriptionResult.TenantSetInActive;
        }

        public override Task UpdateAsync(Tenant tenant)
        {
            if (tenant.IsInTrialPeriod && !tenant.SubscriptionEndDateUtc.HasValue)
            {
                throw new UserFriendlyException(LocalizationManager.GetString(
                    DispatcherWebConsts.LocalizationSourceName, "TrialWithoutEndDateErrorMessage"));
            }

            return base.UpdateAsync(tenant);
        }

        private async Task GrantDefaultPermissionsToStaticRole(string roleName)
        {
            var role = _roleManager.Roles.Single(r => r.Name == roleName);
            await _roleManager.RestoreDefaultPermissionsAsync(role);
        }
    }
}
