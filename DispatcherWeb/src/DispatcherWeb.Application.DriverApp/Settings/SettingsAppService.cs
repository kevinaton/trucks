using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApp.Settings.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.DriverApp.Settings
{
    [AbpAuthorize]
    public class SettingsAppService : DispatcherWebDriverAppAppServiceBase, ISettingsAppService
    {

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Driver> _driverRepository;

        public SettingsAppService(
            IAppConfigurationAccessor configurationAccessor,
            IRepository<Driver> driverRepository
            )
        {
            _appConfiguration = configurationAccessor.Configuration;
            _driverRepository = driverRepository;
        }

        public async Task<SettingsDto> Get()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            var result = new SettingsDto
            {
                //UseShifts = await SettingManager.UseShifts(),
                //UseBackgroundSync = (_appConfiguration["App:UseBackgroundSyncForDriverApp"] ?? "true") != "false",
                HttpRequestTimeout = GetDriverApplicationHttpTimeout(),
                //ShiftNames = (await SettingManager.GetShiftDictionary()).ToDictionary(x => (int)x.Key, x => x.Value),
                HideTicketControls = await SettingManager.HideTicketControlsInDriverApp(),
                RequireToEnterTickets = await SettingManager.RequireDriversToEnterTickets(),
                RequireSignature = await SettingManager.RequireSignature(),
                RequireTicketPhoto = await SettingManager.RequireTicketPhoto(),
                TextForSignatureView = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView),
                //DispatchesLockedToTruck = await SettingManager.DispatchesLockedToTruck(),
                IsUserAdmin = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin)
                    || await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Administrative),
            };

            var driver = await _driverRepository.GetAll()
                .Include(x => x.LeaseHaulerDriver)
                .Where(x => x.UserId == user.Id && !x.IsInactive)
                .Select(x => new
                {
                    x.Id,
                    LeaseHaulerId = (int?)x.LeaseHaulerDriver.LeaseHaulerId
                })
                .FirstOrDefaultAsync();

            if (driver != null)
            {
                result.IsUserDriver = true;
                result.IsUserLeaseHaulerDriver = driver.LeaseHaulerId != null;
            }

            return result;
        }

        private int GetDriverApplicationHttpTimeout()
        {
            const int defaultTimeout = 60000;
            var httpTimeoutString = _appConfiguration["App:DriverApplicationHttpRequestTimeout"];
            if (!string.IsNullOrEmpty(httpTimeoutString) && int.TryParse(httpTimeoutString, out var httpTimeout))
            {
                return httpTimeout;
            }
            return defaultTimeout;
        }
    }
}
