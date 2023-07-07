using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Trucks;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class HomeController : DispatcherWebControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Truck> _truckRepository;

        public HomeController(
            UserManager userManager,
            IRepository<Driver> driverRepository,
            IRepository<Truck> truckRepository
        )
        {
            _userManager = userManager;
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
        }

        public async Task<ActionResult> Index()
        {
            if (AbpSession.MultiTenancySide == MultiTenancySides.Host)
            {
                if (await IsGrantedAsync(AppPermissions.Pages_Administration_Host_Dashboard))
                {
                    return RedirectToAction("Index", "HostDashboard");
                }

                if (await IsGrantedAsync(AppPermissions.Pages_Tenants))
                {
                    return RedirectToAction("Index", "Tenants");
                }
            }
            else
            {
                var hasDashboardPermission = await IsGrantedAsync(AppPermissions.Pages_Dashboard);
                var hasReactNativeDriverAppPermission = await IsGrantedAsync(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp);
                var hasPwaDriverAppPermission = await IsGrantedAsync(AppPermissions.Pages_DriverApplication_WebBasedDriverApp);

                var redirectRelatedPermissionList = new[]
                {
                    hasDashboardPermission,
                    hasReactNativeDriverAppPermission,
                    hasPwaDriverAppPermission,
                };

                if (redirectRelatedPermissionList.Count(x => x == true) == 1)
                {
                    if (hasDashboardPermission)
                    {
                        if (await IsGrantedAsync(AppPermissions.Pages_Schedule))
                        {
                            if (!await _truckRepository.GetAll().AnyAsync())
                            {
                                return RedirectToAction("Index", "Scheduling");
                            }
                        }
                    
                        return RedirectToAction("Index", "Dashboard");
                    }

                    if (hasReactNativeDriverAppPermission)
                    {
                        return RedirectToAction("ReactNative", "DriverApplication");
                    }

                    if (hasPwaDriverAppPermission)
                    {
                        return RedirectToAction("PWA", "DriverApplication");
                    }
                }
                else if (redirectRelatedPermissionList.Any())
                {
                    return RedirectToAction("ChooseRedirectTarget", "Welcome");
                }
            }

            //Default page if no permission to the pages above
            return RedirectToAction("Index", "Welcome");
        }
    }
}