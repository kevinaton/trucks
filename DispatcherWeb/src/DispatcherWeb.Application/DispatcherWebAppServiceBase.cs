using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.Timing;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Exceptions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Sessions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class DispatcherWebAppServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        public AspNetZeroAbpSession Session { get; set; }

        protected DispatcherWebAppServiceBase()
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

        protected async Task<string> GetTimezone(int? tenantId, long userId)
        {
            return await SettingManager.GetSettingValueForUserAsync(TimingSettingNames.TimeZone, tenantId, userId);
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

        protected int OfficeId => Session.GetOfficeIdOrThrow();

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

        protected async Task CheckUseShiftSettingCorrespondsInput(Shift? shift)
        {
            if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts))
            {
                if (!shift.HasValue)
                {
                    throw new ArgumentException("UseShifts is turned on but there are no shifts in the input.");
                }
            }
            else
            {
                if (shift.HasValue)
                {
                    throw new ArgumentException("UseShifts is turned off but there are shifts in the input.");
                }
            }
        }

        protected async Task CheckUseShiftSettingCorrespondsInput(Shift[] shifts)
        {
            if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts))
            {
                if (shifts.IsNullOrEmpty())
                {
                    throw new ArgumentException("UseShifts is turned on but there are no shifts in the input.");
                }
            }
            else
            {
                if (!shifts.IsNullOrEmpty())
                {
                    throw new ArgumentException("UseShifts is turned off but there are shifts in the input.");
                }
            }
        }

        protected async Task<bool> CanEditAnyOrderDirectionsAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                return false;
            }
            var isDispatcher = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Dispatching);
            return isDispatcher;
        }
    }
}