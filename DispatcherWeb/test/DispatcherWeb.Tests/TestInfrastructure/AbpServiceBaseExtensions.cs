using Abp;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Configuration;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using NSubstitute;

namespace DispatcherWeb.Tests.TestInfrastructure
{
    public static class AbpServiceBaseExtensions
    {
        public static ISettingManager SubstituteSetting(this AbpServiceBase service, string settingName, string settingValue)
        {
            var settingManager = GetSettingManager();
            settingManager.GetSettingValueAsync(settingName).Returns(settingValue);
            service.SettingManager = settingManager;
            return settingManager;
        }

        public static ISettingManager SubstituteSettingForUser(this AbpServiceBase service, string settingName, string settingValue, UserIdentifier userIdentifier)
        {
            var settingManager = GetSettingManager();
            settingManager.GetSettingValueForUserAsync(settingName, userIdentifier).Returns(settingValue);
            service.SettingManager = settingManager;
            return settingManager;
        }

        public static ISettingManager SubstituteSettingForTenant(this AbpServiceBase service, string settingName, string settingValue, int tenantId)
        {
            var settingManager = GetSettingManager();
            settingManager.GetSettingValueForTenantAsync(settingName, tenantId).Returns(settingValue);
            service.SettingManager = settingManager;
            return settingManager;
        }

        public static ISettingManager SubstituteDispatchSettings(this AbpServiceBase service, DispatchVia dispatchVia)
        {
            var settingManager = GetSettingManager();
            settingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(((int)dispatchVia).ToString());
            service.SettingManager = settingManager;
            return settingManager;
        }

        private static ISettingManager GetSettingManager()
        {
            var settingManager = Substitute.For<ISettingManager>();
            settingManager.GetSettingValueAsync(TimingSettingNames.TimeZone).Returns("UTC");
            return settingManager;
        }

        public static IFeatureChecker SubstituteAllowLeaseHaulersFeature(this ApplicationService service, bool value)
        {
            var featureChecker = Substitute.For<IFeatureChecker>();
            featureChecker.GetValueAsync(AppFeatures.AllowLeaseHaulersFeature).Returns(value.ToLowerCaseString());
            service.FeatureChecker = featureChecker;
            return featureChecker;
        }

        public static IFeatureChecker SubstituteAllowMultiOfficeFeature(this ApplicationService service, bool value)
        {
            var featureChecker = Substitute.For<IFeatureChecker>();
            featureChecker.GetValueAsync(AppFeatures.AllowMultiOfficeFeature).Returns(value.ToLowerCaseString());
            service.FeatureChecker = featureChecker;
            return featureChecker;
        }

    }
}
