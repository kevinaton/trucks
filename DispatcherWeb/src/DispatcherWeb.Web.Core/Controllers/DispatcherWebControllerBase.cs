using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Configuration.Startup;
using Abp.IdentityFramework;
using Abp.Timing;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DispatcherWeb.Web.Controllers
{
    public abstract class DispatcherWebControllerBase : AbpController
    {
        protected DispatcherWebControllerBase()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        protected void SetTenantIdCookie(int? tenantId)
        {
            var multiTenancyConfig = HttpContext.RequestServices.GetRequiredService<IMultiTenancyConfig>();
            Response.Cookies.Append(
                multiTenancyConfig.TenantIdResolveKey,
                tenantId?.ToString() ?? string.Empty,
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddYears(5),
                    Path = "/"
                }
            );
        }
        protected async Task<DateTime> GetToday()
        {
            var timeZone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            return TimeExtensions.GetToday(timeZone);
        }

        protected FileContentResult InlinePdfFile(byte[] fileContents, string fileName)
        {
            string mimeType = "application/pdf";
            Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName.SanitizeFilename());
            return File(fileContents, mimeType);
        }
    }
}
