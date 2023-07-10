using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Configuration.Host.Dto;

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