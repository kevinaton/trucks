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
            if (user.OfficeId.HasValue)
            {
                if (user.Office != null)
                {
                    officeName = user.Office.Name;
                    officeCopyChargeTo = user.Office.CopyDeliverToLoadAtChargeTo;
                }
                else
                {
                    var userWithOffice = await UserManager.Users
                        .Include(x => x.Office)
                        .Where(x => x.Id == user.Id)
                        .Select(x => new
                        {
                            OfficeName = x.Office.Name,
                            OfficeCopyChargeTo = x.Office.CopyDeliverToLoadAtChargeTo
                        })
                        .FirstOrDefaultAsync();
                    officeName = userWithOffice?.OfficeName;
                    officeCopyChargeTo = userWithOffice?.OfficeCopyChargeTo ?? false;
                }
            }

            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeId, user.OfficeId + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeName, officeName ?? ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeCopyChargeTo, officeCopyChargeTo ? "true" : "false"));
            }

            return principal;
        }
    }
}
