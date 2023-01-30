using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration.Startup;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;
using DispatcherWeb.Runtime.Session;
using NSubstitute;

namespace DispatcherWeb.UnitTests.TestUtilities
{
    public static class SessionHelper
    {
        public static AspNetZeroAbpSession CreateSession()
        {
            var principalAccessor = Substitute.For<IPrincipalAccessor>();
            List<Claim> claims = new List<Claim>()
            {
                new Claim(DispatcherWebConsts.Claims.UserOfficeId, "1"),
            };
            var claimsPrincipal = Substitute.For<ClaimsPrincipal>();
            claimsPrincipal.Claims.Returns(claims);
            principalAccessor.Principal.Returns(claimsPrincipal);
            var multiTenancy = Substitute.For<IMultiTenancyConfig>();
            var tenantResolver = Substitute.For<ITenantResolver>();
            var sessionOverrideScopeProvider = Substitute.For<IAmbientScopeProvider<SessionOverride>>();
            var session = new AspNetZeroAbpSession(principalAccessor, multiTenancy, tenantResolver, sessionOverrideScopeProvider);
            return session;
        }

    }
}
