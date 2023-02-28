using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;

namespace DispatcherWeb.Web.Controllers
{
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users)]
    public class UsersController : UsersControllerBase
    {
        public UsersController()
        {
        }
    }
}