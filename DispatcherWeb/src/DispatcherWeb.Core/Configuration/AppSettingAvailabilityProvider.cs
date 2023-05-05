﻿using System.Threading.Tasks;
using Abp.Application.Features;
using DispatcherWeb.Features;

namespace DispatcherWeb.Configuration
{
    public class AppSettingAvailabilityProvider : IAppSettingAvailabilityProvider
    {
        public AppSettingAvailabilityProvider(
            IFeatureChecker featureChecker
            )
        {
            FeatureChecker = featureChecker;
        }

        public IFeatureChecker FeatureChecker { get; }

        public async Task<bool> IsSettingAvailableAsync(string settingName)
        {
            switch (settingName)
            {
                case AppSettings.DispatchingAndMessaging.DispatchVia:
                case AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp:
                case AppSettings.DispatchingAndMessaging.RequireDriversToEnterTickets:
                case AppSettings.DispatchingAndMessaging.RequireSignature:
                case AppSettings.DispatchingAndMessaging.RequireTicketPhoto:
                case AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck:
                case AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant:
                case AppSettings.DispatchingAndMessaging.TextForSignatureView:
                    return await FeatureChecker.IsEnabledAsync(AppFeatures.DispatchingFeature);

                case AppSettings.DispatchingAndMessaging.SendSmsOnDispatching:
                case AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate:
                    return await FeatureChecker.IsEnabledAsync(true, AppFeatures.DispatchingFeature, AppFeatures.SmsIntegrationFeature);

                case AppSettings.DispatchingAndMessaging.AllowSmsMessages:
                case AppSettings.DispatchingAndMessaging.SmsPhoneNumber:
                    return await FeatureChecker.IsEnabledAsync(AppFeatures.SmsIntegrationFeature);

                case AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule:
                case AppSettings.DispatchingAndMessaging.ValidateUtilization:
                case AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate:
                case AppSettings.DispatchingAndMessaging.DefaultStartTime:
                case AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers:
                    return true;

                default:
                    return true;
            }
        }
    }
}
