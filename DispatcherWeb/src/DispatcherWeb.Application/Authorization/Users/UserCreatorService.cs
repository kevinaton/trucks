using Abp.Application.Services;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Authorization.Users
{
    public class UserCreatorService : DispatcherWebAppServiceBase, IUserCreatorService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IUserEmailer _userEmailer;
        private readonly IUserPolicy _userPolicy;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;

        public UserCreatorService(
            RoleManager roleManager,
            UserManager userManager,
            IUserEmailer userEmailer,
            IUserPolicy userPolicy,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            IPasswordHasher<User> passwordHasher,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier
            )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userEmailer = userEmailer;
            _userPolicy = userPolicy;
            _passwordValidators = passwordValidators;
            _passwordHasher = passwordHasher;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            AppUrlService = NullAppUrlService.Instance;
        }

        [RemoteService(IsEnabled = false)]
        public async Task<User> CreateUser(CreateOrUpdateUserInput input)
        {
            if (AbpSession.TenantId.HasValue)
            {
                await _userPolicy.CheckMaxUserCountAsync(AbpSession.GetTenantId());
            }

            var user = new User
            {
                TenantId = AbpSession.TenantId,
                Name = input.User.Name,
                Surname = input.User.Surname,
                UserName = input.User.UserName,
                EmailAddress = input.User.EmailAddress,
                PhoneNumber = input.User.PhoneNumber,
                OfficeId = input.User.OfficeId,
                IsActive = input.User.IsActive,
                ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin,
                IsTwoFactorEnabled = input.User.IsTwoFactorEnabled,
                IsLockoutEnabled = input.User.IsLockoutEnabled,
            };

            //Set password
            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                foreach (var validator in _passwordValidators)
                {
                    CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                }
                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }
            else
            {
                throw new UserFriendlyException("Password is required");
            }

            user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;

            //Assign roles
            user.Roles = new Collection<UserRole>();
            foreach (var roleName in input.AssignedRoleNames)
            {
                var role = await _roleManager.GetRoleByNameAsync(roleName);
                user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            }

            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.

            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            await _appNotifier.WelcomeToTheApplicationAsync(user);

            //Organization Units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            //Send activation email
            if (input.SendActivationEmail)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    input.User.Password
                );
            }

            return user;
        }
    }
}
