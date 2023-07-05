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
using DispatcherWeb.Dispatching;
using DispatcherWeb.Features;
using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.CustomerContacts
{
    public class CustomerContactUserLinkService : DispatcherWebDomainServiceBase, ICustomerContactUserLinkService
    {
        private readonly IRepository<CustomerContact> _CustomerContactRepository;
        private readonly IUserCreatorService _userCreatorService;

        public CustomerContactUserLinkService(
            IRepository<CustomerContact> CustomerContactRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IUserCreatorService userCreatorService
        )
        {
            _CustomerContactRepository = CustomerContactRepository;
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

            var linkedCustomerContact = await _CustomerContactRepository.GetAll()
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
                user = await _userCreatorService.CreateUser(new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        CustomerContactId = customerContact.Id,
                        EmailAddress = customerContact.Email,
                        Title = customerContact.Title,
                        Name = customerContact.FirstName,
                        Surname = customerContact.LastName,
                        PhoneNumber = customerContact.PhoneNumber,
                        UserName = customerContact.Email.Split("@").First(),
                        IsActive = true,
                        IsLockoutEnabled = true,
                        ShouldChangePasswordOnNextLogin = false
                    },
                    AssignedRoleNames = new[] { StaticRoleNames.Tenants.Customer },
                    SendActivationEmail = sendEmail,
                    SetRandomPassword = true
                });

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

            user.Name = customerContact.FirstName;
            user.Surname = customerContact.LastName;
            user.Title = customerContact.Title;
            user.EmailAddress = customerContact.Email;
            user.PhoneNumber = customerContact.PhoneNumber;
            user.OfficeId = null;
            user.CustomerContactId = customerContact.Id;

            (await UserManager.UpdateAsync(user)).CheckErrors(LocalizationManager);
        }

        private async Task UpdateCustomerContactFromUser(CustomerContact customerContact, User user)
        {
            customerContact.FirstName = user.Name;
            customerContact.LastName = user.Surname;
            customerContact.Title = user.Title;
            customerContact.Email = user.EmailAddress;
            customerContact.PhoneNumber = user.PhoneNumber;
            await _CustomerContactRepository.UpdateAsync(customerContact);
        }

        #endregion private methods
    }
}
