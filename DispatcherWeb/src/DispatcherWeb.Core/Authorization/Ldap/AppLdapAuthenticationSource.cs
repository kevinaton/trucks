using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.Authorization.Ldap
{
    public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
    {
        public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
            : base(settings, ldapModuleConfig)
        {
        }
    }
}