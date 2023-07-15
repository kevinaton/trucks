using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Customers;
using DispatcherWeb.Features;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.CustomerContacts
{
    public class CustomerContactUserLinkService : DispatcherWebDomainServiceBase, ICustomerContactUserLinkService
    {
        private readonly IRepository<CustomerContact> _customerContactRepository;
        private readonly IUserCreatorService _userCreatorService;

        public CustomerContactUserLinkService(
            IRepository<CustomerContact> CustomerContactRepository,
            IUserCreatorService userCreatorService
        )
        {
            _customerContactRepository = CustomerContactRepository;
            _userCreatorService = userCreatorService;
        }

        /// <summary>
        /// Should be called when a user is modified
        /// </summary>
        public async Task UpdateCustomerContact(User user)
        {
            if (user.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("EmailAddress is required");
            }

            if (user.Id == 0)
            {
                throw new ArgumentException("User should be saved first", nameof(user));
            }

            var userIsInCustomerRole = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Customer);
            var customerPortalFeatureEnabled = await FeatureChecker.IsEnabledAsync(AppFeatures.CustomerPortal);
            if (!userIsInCustomerRole || !customerPortalFeatureEnabled)
            {
                //throw new UserFriendlyException("Customer portal feature is not enabled for this user.");
                return;
            }

            var linkedCustomerContact = await _customerContactRepository.GetAll()
                                            .FirstOrDefaultAsync(x => x.Id == user.CustomerContactId);

            if (linkedCustomerContact != null)
            {
                await UpdateCustomerContactFromUser(linkedCustomerContact, user);
            }
        }

        /// <summary>
        /// Should be called when a CustomerContact is modified
        /// </summary>
        public async Task<User> UpdateUser(CustomerContact customerContact, bool sendEmail = true)
        {
            var linkedUser = await UserManager.Users.FirstOrDefaultAsync(x => x.CustomerContactId == customerContact.Id);
            if (linkedUser != null)
            {
                if (!await IsUserInCustomerContactRole(linkedUser))
                {
                    await UserManager.AddToRoleAsync(linkedUser, StaticRoleNames.Tenants.Customer);
                }
                await UpdateUserFromCustomerContactAsync(linkedUser, customerContact);
                return linkedUser;
            }

            var user = await UserManager.FindByEmailAsync(customerContact.Email);
            if (user != null)
            {
                if (!await IsUserInCustomerContactRole(user))
                {
                    await UserManager.AddToRoleAsync(user, StaticRoleNames.Tenants.Customer);
                }
                await UpdateUserFromCustomerContactAsync(user, customerContact);
                return user;
            }
            else
            {
                var nameParts = customerContact.Name.Split(' ');
                var newUser = new UserEditDto
                {
                    CustomerContactId = customerContact.Id,
                    EmailAddress = customerContact.Email,
                    Name = nameParts.First(),
                    Surname = nameParts.Length > 1 ? nameParts[1] : nameParts.First(),
                    PhoneNumber = customerContact.PhoneNumber,
                    UserName = customerContact.Email.Split("@").First(),
                    IsActive = true,
                    IsLockoutEnabled = true,
                    ShouldChangePasswordOnNextLogin = false
                };

                var createOrUpdateUserInput = new CreateOrUpdateUserInput
                {
                    User = newUser,
                    AssignedRoleNames = new[] { StaticRoleNames.Tenants.Customer },
                    SendActivationEmail = sendEmail,
                    SetRandomPassword = true
                };

                user = await _userCreatorService.CreateUser(createOrUpdateUserInput);

                return user;
            }
        }

        #region private methods

        private async Task<bool> IsUserInCustomerContactRole(User user)
        {
            return await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Customer);
        }

        private async Task UpdateUserFromCustomerContactAsync(User user, CustomerContact customerContact)
        {
            if (customerContact.Email.IsNullOrEmpty())
            {
                throw new UserFriendlyException("EmailAddress is required");
            }

            var nameParts = customerContact.Name.Split(' ');
            user.Name = nameParts.First();
            user.Surname = nameParts.Length > 1 ? nameParts[1] : nameParts.First();
            user.Title = customerContact.Title;
            user.EmailAddress = customerContact.Email;
            user.PhoneNumber = customerContact.PhoneNumber;
            user.CustomerContactId = customerContact.Id;

            (await UserManager.UpdateAsync(user)).CheckErrors(LocalizationManager);
        }

        private async Task UpdateCustomerContactFromUser(CustomerContact customerContact, User user)
        {
            customerContact.Name = $"{user.Name} {user.Surname}";
            customerContact.Title = user.Title;
            customerContact.Email = user.EmailAddress;
            customerContact.PhoneNumber = user.PhoneNumber;
            await _customerContactRepository.UpdateAsync(customerContact);
        }

        #endregion private methods
    }
}
