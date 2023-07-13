using System.Linq;
using Abp.Configuration.Startup;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;

namespace DispatcherWeb.Runtime.Session
{
    /// <summary>
    /// Extends <see cref="ClaimsAbpSession"/>  (from Abp library).
    /// </summary>
    public class AspNetZeroAbpSession : ClaimsAbpSession
    {
        //public ITenantIdAccessor TenantIdAccessor { get; set; }

        public AspNetZeroAbpSession(
            IPrincipalAccessor principalAccessor,
            IMultiTenancyConfig multiTenancy,
            ITenantResolver tenantResolver,
            IAmbientScopeProvider<SessionOverride> sessionOverrideScopeProvider
        ) : base(principalAccessor, multiTenancy, tenantResolver, sessionOverrideScopeProvider)
        {
        }

        public int? OfficeId
        {
            get
            {
                var claimsPrincipal = PrincipalAccessor.Principal; //Thread.CurrentPrincipal as ClaimsPrincipal;

                var officeIdClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == DispatcherWebConsts.Claims.UserOfficeId);
                if (string.IsNullOrEmpty(officeIdClaim?.Value))
                {
                    return null;
                }

                return int.Parse(officeIdClaim.Value);
            }
        }

        public string OfficeName
        {
            get
            {
                var claimsPrincipal = PrincipalAccessor.Principal;

                var officeNameClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == DispatcherWebConsts.Claims.UserOfficeName);
                if (string.IsNullOrEmpty(officeNameClaim?.Value))
                {
                    return null;
                }
                return officeNameClaim.Value;
            }
        }

        public bool OfficeCopyChargeTo
        {
            get
            {
                var claimsPrincipal = PrincipalAccessor.Principal;

                var officeCopyChargeToClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == DispatcherWebConsts.Claims.UserOfficeCopyChargeTo);
                if (string.IsNullOrEmpty(officeCopyChargeToClaim?.Value))
                {
                    return false;
                }
                return officeCopyChargeToClaim.Value == "true";
            }
        }

        public int? CustomerId
        {
            get
            {
                var claimsPrincipal = PrincipalAccessor.Principal; 

                var customerIdClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == DispatcherWebConsts.Claims.UserCustomerId);
                if (string.IsNullOrEmpty(customerIdClaim?.Value))
                {
                    return null;
                }

                return int.Parse(customerIdClaim.Value);
            }
        }

        public string CustomerName
        {
            get
            {
                var claimsPrincipal = PrincipalAccessor.Principal;

                var customerNameClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == DispatcherWebConsts.Claims.UserCustomerName);
                if (string.IsNullOrEmpty(customerNameClaim?.Value))
                {
                    return null;
                }
                return customerNameClaim.Value;
            }
        }

        public bool? HasCustomerPortalAccess
        {
            get
            {
                var claimsPrincipal = PrincipalAccessor.Principal;

                var hasCustomerPortalAccessClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == DispatcherWebConsts.Claims.UserHasCustomerPortalAccess);
                if (string.IsNullOrEmpty(hasCustomerPortalAccessClaim?.Value))
                {
                    return null;
                }
                return bool.Parse(hasCustomerPortalAccessClaim.Value);
            }
        }
    }
}