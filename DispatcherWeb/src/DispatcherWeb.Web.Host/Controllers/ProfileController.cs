using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization.Users.Profile;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ProfileController : ProfileControllerBase
    {
        public ProfileController(
            ITempFileCacheManager tempFileCacheManager,
            IProfileAppService profileAppService) :
            base(tempFileCacheManager, profileAppService)
        {
        }
    }
}
