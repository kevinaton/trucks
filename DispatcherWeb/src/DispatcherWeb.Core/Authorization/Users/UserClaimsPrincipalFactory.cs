using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Uow;
using DispatcherWeb.Authorization.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DispatcherWeb.Authorization.Users
{
    public class UserClaimsPrincipalFactory : AbpUserClaimsPrincipalFactory<User, Role>
    {
        public UserClaimsPrincipalFactory(
            UserManager userManager,
            RoleManager roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IUnitOfWorkManager unitOfWorkManager)
            : base(
                  userManager,
                  roleManager,
                  optionsAccessor,
                  unitOfWorkManager)
        {

        }
        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);

            bool officeCopyChargeTo = false;
            string officeName = null;

            int? customerId = null;
            string customerName = null;
            bool? hasCustomerPortalAccess = null;

            if (user.OfficeId.HasValue || user.CustomerContactId.HasValue)
            {
                var userWithOfficeAndCustomerContact = await UserManager.Users
                    .Where(x => x.Id == user.Id)
                    .Select(x => new
                    {
                        OfficeName = x.Office.Name,
                        OfficeCopyChargeTo = x.Office.CopyDeliverToLoadAtChargeTo,
                        CustomerId = (int?)x.CustomerContact.Customer.Id,
                        CustomerName = x.CustomerContact.Customer.Name,
                        HasCustomerPortalAccess = (bool?)x.CustomerContact.HasCustomerPortalAccess
                    })
                    .FirstOrDefaultAsync();

                officeName = userWithOfficeAndCustomerContact?.OfficeName;
                officeCopyChargeTo = userWithOfficeAndCustomerContact?.OfficeCopyChargeTo ?? false;
                customerId = userWithOfficeAndCustomerContact.CustomerId;
                customerName = userWithOfficeAndCustomerContact.CustomerName;
                hasCustomerPortalAccess = userWithOfficeAndCustomerContact.HasCustomerPortalAccess;
            }

            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeId, user.OfficeId + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeName, officeName ?? ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeCopyChargeTo, officeCopyChargeTo ? "true" : "false"));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserCustomerId, customerId + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserCustomerName, customerName + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserHasCustomerPortalAccess, (hasCustomerPortalAccess ?? false) ? "true" : "false"));
            }

            return principal;
        }
    }
}
