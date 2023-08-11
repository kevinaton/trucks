using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Timing.Timezone;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using AppSettingsConfig = DispatcherWeb.Configuration.AppSettings;
using DispatcherWeb.UserSettings.Dto;

namespace DispatcherWeb.UserSettings
{
    [AbpAuthorize]
    public class UserSettingsAppService : DispatcherWebAppServiceBase, IUserSettingsAppService
    {
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        private readonly IScopedIocResolver _scopedIocResolver;

        public UserSettingsAppService(
            ISettingDefinitionManager settingDefinitionManager,
            IScopedIocResolver scopedIocResolver
            )
        {
            _settingDefinitionManager = settingDefinitionManager;
            _scopedIocResolver = scopedIocResolver;
        }

        public async Task<UserConfig> GetUserAppConfig()
        {
            var config = new UserConfig
            {
                Permissions = new UserPermission
                {
                    Edit = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Orders_Edit),
                    Print = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_PrintOrders),
                    EditTickets = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Tickets_Edit),
                    EditQuotes = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Quotes_Edit),
                    DriverMessages = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_DriverMessages),
                    Trucks = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Trucks)
                },
                Features = new UserFeatures
                {
                    AllowSharedOrders = await base.IsEnabledAsync(AppFeatures.AllowSharedOrdersFeature),
                    AllowMultiOffice = await base.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature),
                    AllowSendingOrdersToDifferentTenant = await base.IsEnabledAsync(AppFeatures.AllowSendingOrdersToDifferentTenant),
                    AllowImportingTruxEarnings = await base.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings),
                    LeaseHaulers = await base.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature),
                },
                Settings = new UserAppSettings
                {
                    ValidateUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettingsConfig.DispatchingAndMessaging.ValidateUtilization),
                    AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders = await SettingManager.GetSettingValueAsync<bool>(AppSettingsConfig.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders),
                    ShowTrailersOnSchedule = await SettingManager.GetSettingValueAsync<bool>(AppSettingsConfig.DispatchingAndMessaging.ShowTrailersOnSchedule),
                    AllowSubcontractorsToDriveCompanyOwnedTrucks = await SettingManager.GetSettingValueAsync<bool>(AppSettingsConfig.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks),
                    AllowSchedulingTrucksWithoutDrivers = await SettingManager.GetSettingValueAsync<bool>(AppSettingsConfig.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers)
                }
            };

            return config;
        }

        public async Task<GeneralSettingsEditDto> GetGeneralSettings()
        {
            var settings = new GeneralSettingsEditDto();
            settings.OrderEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailSubjectTemplate);
            settings.OrderEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailBodyTemplate);
            settings.ReceiptEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailSubjectTemplate);
            settings.ReceiptEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailBodyTemplate);
            settings.CompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
            settings.DefaultMapLocationAddress = await SettingManager.GetSettingValueAsync(AppSettings.General.DefaultMapLocationAddress);
            settings.DefaultMapLocation = await SettingManager.GetSettingValueAsync(AppSettings.General.DefaultMapLocation);
            settings.CurrencySymbol = await SettingManager.GetSettingValueAsync(AppSettings.General.CurrencySymbol);
            settings.UserDefinedField1 = await SettingManager.GetSettingValueAsync(AppSettings.General.UserDefinedField1);
            settings.DontValidateDriverAndTruckOnTickets = !await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ValidateDriverAndTruckOnTickets);
            settings.ShowDriverNamesOnPrintedOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowDriverNamesOnPrintedOrder);
            settings.SplitBillingByOffices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices);

            settings.UseShifts = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts);
            settings.ShiftName1 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName1);
            settings.ShiftName2 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName2);
            settings.ShiftName3 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName3);

            settings.DriverOrderEmailTitle = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailTitle);
            settings.DriverOrderEmailBody = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailBody);
            settings.DriverOrderSms = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.Sms);

            if (Clock.SupportsMultipleTimezone)
            {
                var timezone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
                settings.Timezone = timezone;
                settings.TimezoneForComparison = timezone;
                settings.TimezoneIana = TimezoneHelper.WindowsToIana(timezone);
            }

            return settings;
        }

        public async Task<string> GetUserSettingByName(string settingName)
        {
            var settingDefinition = _settingDefinitionManager.GetSettingDefinition(settingName);
            if (settingDefinition.ClientVisibilityProvider == null 
                || !await settingDefinition.ClientVisibilityProvider.CheckVisible(_scopedIocResolver))
            {
                throw new ApplicationException($"Setting {settingName} is not set to be client-side visible");
            }

            return SettingManager.GetSettingValue(settingName);
        }
    }
}