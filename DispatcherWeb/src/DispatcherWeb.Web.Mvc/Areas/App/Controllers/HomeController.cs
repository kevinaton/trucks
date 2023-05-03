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
                if (await IsGrantedAsync(AppPermissions.Pages_Schedule))
                {
                    if (!await _truckRepository.GetAll().AnyAsync())
                    {
                        return RedirectToAction("Index", "Scheduling");
                    }
                }

                if (await IsGrantedAsync(AppPermissions.Pages_Dashboard))
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                if (await UserIsDriver())
                {
                    return RedirectToAction("Index", "DriverApplication");
                }
            }

            //Default page if no permission to the pages above
            return RedirectToAction("Index", "Welcome");

            // Local functions
            async Task<bool> UserIsDriver()
            {
                var user = await _userManager.GetUserAsync(AbpSession.ToUserIdentifier());

                return await HasDriverRole() && await DriverIsAssociatedWithUser();

                async Task<bool> HasDriverRole() => await _userManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Driver)
                    || await _userManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);
                async Task<bool> DriverIsAssociatedWithUser() => await _driverRepository.GetAll().AnyAsync(d => d.UserId == user.Id);
            }
        }
    }
}