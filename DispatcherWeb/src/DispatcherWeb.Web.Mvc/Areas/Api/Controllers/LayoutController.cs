using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DispatcherWeb.Web.Areas.Api.Controllers
{
    [Area("api")]
    [AbpMvcAuthorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LayoutController : AbpController
    {
        [Route("get-support-link")]
        public async Task<IActionResult> GetSupportLinkAddress()
        {
            var result = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.SupportLinkAddress);
            return Ok(result);
        }
    }
}