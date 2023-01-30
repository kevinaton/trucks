using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.IdentityFramework;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.Timing;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Exceptions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Sessions;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Authorization.Roles;

namespace DispatcherWeb.DriverApp
{
    /// <summary>
    /// Derive your driver app application services from this class.
    /// </summary>
    public abstract class DispatcherWebDriverAppAppServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        public AspNetZeroAbpSession Session { get; set; }

        protected DispatcherWebDriverAppAppServiceBase()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        protected virtual User GetCurrentUser()
        {
            return AsyncHelper.RunSync(GetCurrentUserAsync);
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
            }
        }

        protected async Task<string> GetTimezone()
        {
            return await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
        }

        protected async Task<DateTime> GetToday()
        {
            var timeZone = await GetTimezone();
            return TimeExtensions.GetToday(timeZone);
        }

        protected virtual Tenant GetCurrentTenant()
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                return TenantManager.GetById(AbpSession.GetTenantId());
            }
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        protected int OfficeId
        {
            get
            {
                if (Session.OfficeId.HasValue)
                {
                    return Session.OfficeId.Value;
                }
                throw new UserFriendlyException("You must have an assigned Office in User Details to use that function");
            }
        }

        protected async Task SaveOrThrowConcurrencyErrorAsync()
        {
            try
            {
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
        }
    }
}