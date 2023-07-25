using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Abp.Net.Mail;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Timing.Timezone;
using Abp.UI;
using Abp.Zero.Configuration;
using Abp.Zero.Ldap.Configuration;
using DispatcherWeb.Authentication;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration.Dto;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.Configuration.Tenants.Dto;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.Locations;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Security;
using DispatcherWeb.Services;
using DispatcherWeb.Storage;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Timing;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Configuration.Tenants
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Tenant_Settings)]
    public class TenantSettingsAppService : SettingsAppServiceBase, ITenantSettingsAppService
    {
        public IExternalLoginOptionsCacheManager ExternalLoginOptionsCacheManager { get; set; }

        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppSettingAvailabilityProvider _settingAvailabilityProvider;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IAppNotifier _appNotifier;
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IDtdTrackerTelematics _dtdTrackerTelematics;
        private readonly IAbpZeroLdapModuleConfig _ldapModuleConfig;

        public TenantSettingsAppService(
            IAbpZeroLdapModuleConfig ldapModuleConfig,
            IMultiTenancyConfig multiTenancyConfig,
            ITimeZoneService timeZoneService,
            IEmailSender emailSender,
            IBinaryObjectManager binaryObjectManager,
            IAppConfigurationAccessor configurationAccessor,
            IAppSettingAvailabilityProvider settingAvailabilityProvider,
            IBackgroundJobManager backgroundJobManager,
            IAppNotifier appNotifier,
            ISettingDefinitionManager settingDefinitionManager,
            IRepository<Driver> driverRepository,
            IRepository<Customer> customerRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Service> serviceRepository,
            IRepository<Office> officeRepository,
            IRepository<Location> locationRepository,
            IDtdTrackerTelematics dtdTrackerTelematics
            ) : base(emailSender, configurationAccessor)
        {
            ExternalLoginOptionsCacheManager = NullExternalLoginOptionsCacheManager.Instance;

            _multiTenancyConfig = multiTenancyConfig;
            _ldapModuleConfig = ldapModuleConfig;
            _timeZoneService = timeZoneService;
            _binaryObjectManager = binaryObjectManager;
            _settingAvailabilityProvider = settingAvailabilityProvider;
            _backgroundJobManager = backgroundJobManager;
            _appNotifier = appNotifier;
            _settingDefinitionManager = settingDefinitionManager;
            _driverRepository = driverRepository;
            _customerRepository = customerRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _orderLineRepository = orderLineRepository;
            _serviceRepository = serviceRepository;
            _officeRepository = officeRepository;
            _locationRepository = locationRepository;
            _dtdTrackerTelematics = dtdTrackerTelematics;
        }

        #region Get Settings

        public async Task<TenantSettingsEditDto> GetAllSettings()
        {
            var settings = new TenantSettingsEditDto
            {
                UserManagement = await GetUserManagementSettingsAsync(),
                Security = await GetSecuritySettingsAsync(),
                Billing = await GetBillingSettingsAsync(),
                OtherSettings = await GetOtherSettingsAsync(),
                Email = await GetEmailSettingsAsync(),
                ExternalLoginProviderSettings = await GetExternalLoginProviderSettings()
            };

            if (!_multiTenancyConfig.IsEnabled || Clock.SupportsMultipleTimezone)
            {
                settings.General = await GetGeneralSettingsAsync();
            }

            if (!_multiTenancyConfig.IsEnabled)
            {
                settings.Email = await GetEmailSettingsAsync();

                if (_ldapModuleConfig.IsEnabled)
                {
                    settings.Ldap = await GetLdapSettingsAsync();
                }
                else
                {
                    settings.Ldap = new LdapSettingsEditDto { IsModuleEnabled = false };
                }
                settings.Sms = await SettingManager.GetSmsSettingsAsync();

                settings.General.WebSiteRootAddress = await SettingManager.GetSettingValueAsync(AppSettings.General.WebSiteRootAddress);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                settings.GpsIntegration = await GetGpsIntegrationSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowPaymentProcessingFeature))
            {
                settings.Payment = await GetPaymentSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings))
            {
                settings.Trux = await GetTruxSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingLuckStoneEarnings))
            {
                settings.LuckStone = await GetLuckStoneSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(false, AppFeatures.GpsIntegrationFeature, AppFeatures.SmsIntegrationFeature, AppFeatures.DispatchingFeature))
            {
                settings.DispatchingAndMessaging = await GetDispatchingAndMessagingSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature))
            {
                settings.LeaseHaulers = await GetLeaseHaulerSettingsEditDto();
            }

            settings.TimeAndPay = await GetTimeAndPaySettingsAsync();

            settings.Fuel = await GetFuelSettingsAsync();

            settings.Quote = await GetQuoteSettingsAsync();

            if (_ldapModuleConfig.IsEnabled)
            {
                settings.Ldap = await GetLdapSettingsAsync();
            }
            else
            {
                settings.Ldap = new LdapSettingsEditDto { IsModuleEnabled = false };
            }

            return settings;
        }

        private async Task<LdapSettingsEditDto> GetLdapSettingsAsync()
        {
            return new LdapSettingsEditDto
            {
                IsModuleEnabled = true,
                IsEnabled = await SettingManager.GetSettingValueForTenantAsync<bool>(LdapSettingNames.IsEnabled, AbpSession.GetTenantId()),
                Domain = await SettingManager.GetSettingValueForTenantAsync(LdapSettingNames.Domain, AbpSession.GetTenantId()),
                UserName = await SettingManager.GetSettingValueForTenantAsync(LdapSettingNames.UserName, AbpSession.GetTenantId()),
                Password = await SettingManager.GetSettingValueForTenantAsync(LdapSettingNames.Password, AbpSession.GetTenantId()),
            };
        }

        private async Task<TenantEmailSettingsEditDto> GetEmailSettingsAsync()
        {
            var useHostDefaultEmailSettings = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.Email.UseHostDefaultEmailSettings, AbpSession.GetTenantId());

            if (useHostDefaultEmailSettings)
            {
                return new TenantEmailSettingsEditDto
                {
                    UseHostDefaultEmailSettings = true
                };
            }

            var smtpPassword = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Password, AbpSession.GetTenantId());

            return new TenantEmailSettingsEditDto
            {
                UseHostDefaultEmailSettings = false,
                DefaultFromAddress = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.DefaultFromAddress, AbpSession.GetTenantId()),
                DefaultFromDisplayName = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.DefaultFromDisplayName, AbpSession.GetTenantId()),
                SmtpHost = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Host, AbpSession.GetTenantId()),
                SmtpPort = await SettingManager.GetSettingValueForTenantAsync<int>(EmailSettingNames.Smtp.Port, AbpSession.GetTenantId()),
                SmtpUserName = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.UserName, AbpSession.GetTenantId()),
                SmtpPassword = SimpleStringCipher.Instance.Decrypt(smtpPassword),
                SmtpDomain = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Domain, AbpSession.GetTenantId()),
                SmtpEnableSsl = await SettingManager.GetSettingValueForTenantAsync<bool>(EmailSettingNames.Smtp.EnableSsl, AbpSession.GetTenantId()),
                SmtpUseDefaultCredentials = await SettingManager.GetSettingValueForTenantAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials, AbpSession.GetTenantId())
            };
        }

        private async Task<GeneralSettingsEditDto> GetGeneralSettingsAsync()
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
            settings.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders);

            settings.UseShifts = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts);
            settings.ShiftName1 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName1);
            settings.ShiftName2 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName2);
            settings.ShiftName3 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName3);

            settings.DriverOrderEmailTitle = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailTitle);
            settings.DriverOrderEmailBody = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailBody);
            settings.DriverOrderSms = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.Sms);

            if (Clock.SupportsMultipleTimezone)
            {
                var timezone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, AbpSession.GetTenantId());

                settings.Timezone = timezone;
                settings.TimezoneForComparison = timezone;
                settings.TimezoneIana = TimezoneHelper.WindowsToIana(timezone);
            }

            var defaultTimeZoneId = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Tenant, AbpSession.TenantId);

            if (settings.Timezone == defaultTimeZoneId)
            {
                settings.Timezone = string.Empty;
            }

            return settings;
        }

        private async Task<TimeAndPaySettingsEditDto> GetTimeAndPaySettingsAsync()
        {
            var settings = new TimeAndPaySettingsEditDto
            {
                AllowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature),
                DefaultToProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay),
                PreventProductionPayOnHourlyJobs = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs),
                AllowDriverPayRateDifferentFromFreightRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate),
                TimeTrackingDefaultTimeClassificationId = await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId),
                DriverIsPaidForLoadBasedOn = (DriverIsPaidForLoadBasedOnEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.DriverIsPaidForLoadBasedOn)
            };

            if (settings.TimeTrackingDefaultTimeClassificationId > 0)
            {
                var timeClassification = await _timeClassificationRepository.GetAll()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
                    })
                    .FirstOrDefaultAsync(x => x.Id == settings.TimeTrackingDefaultTimeClassificationId);

                if (timeClassification == null)
                {
                    settings.TimeTrackingDefaultTimeClassificationId = 0;
                }
                else
                {
                    settings.TimeTrackingDefaultTimeClassificationName = timeClassification.Name;
                }
            }

            return settings;
        }

        private async Task<TenantUserManagementSettingsEditDto> GetUserManagementSettingsAsync()
        {
            return new TenantUserManagementSettingsEditDto
            {
                AllowSelfRegistration = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowSelfRegistration),
                IsNewRegisteredUserActiveByDefault = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault),
                IsEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin),
                UseCaptchaOnRegistration = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnRegistration),
                UseCaptchaOnLogin = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnLogin),
                IsCookieConsentEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsCookieConsentEnabled),
                IsQuickThemeSelectEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsQuickThemeSelectEnabled),
                AllowUsingGravatarProfilePicture = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowUsingGravatarProfilePicture),
                SessionTimeOutSettings = new SessionTimeOutSettingsEditDto()
                {
                    IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SessionTimeOut.IsEnabled),
                    TimeOutSecond = await SettingManager.GetSettingValueAsync<int>(AppSettings.UserManagement.SessionTimeOut.TimeOutSecond),
                    ShowTimeOutNotificationSecond = await SettingManager.GetSettingValueAsync<int>(AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond),
                    ShowLockScreenWhenTimedOut = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut)
                }
            };
        }

        private async Task<SecuritySettingsEditDto> GetSecuritySettingsAsync()
        {
            var passwordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit),
                RequireLowercase = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase),
                RequireNonAlphanumeric = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric),
                RequireUppercase = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase),
                RequiredLength = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength)
            };

            var defaultPasswordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit),
                RequireLowercase = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase),
                RequireNonAlphanumeric = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric),
                RequireUppercase = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase),
                RequiredLength = await SettingManager.GetSettingValueForApplicationAsync<int>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength)
            };

            return new SecuritySettingsEditDto
            {
                UseDefaultPasswordComplexitySettings = passwordComplexitySetting.Equals(defaultPasswordComplexitySetting),
                PasswordComplexity = passwordComplexitySetting,
                DefaultPasswordComplexity = defaultPasswordComplexitySetting,
                UserLockOut = await GetUserLockOutSettingsAsync(),
                TwoFactorLogin = await GetTwoFactorLoginSettingsAsync(),
                AllowOneConcurrentLoginPerUser = await GetOneConcurrentLoginPerUserSetting()
            };
        }

        private async Task<TenantBillingSettingsEditDto> GetBillingSettingsAsync()
        {
            var autopopulateDefaultTaxRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.AutopopulateDefaultTaxRate);
            return new TenantBillingSettingsEditDto()
            {
                LegalName = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingLegalName),
                Address = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingAddress),
                PhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingPhoneNumber),
                RemitToInformation = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.RemitToInformation),
                DefaultMessageOnInvoice = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.DefaultMessageOnInvoice),
                InvoiceEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailSubjectTemplate),
                InvoiceEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailBodyTemplate),
                TaxVatNo = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingTaxVatNo),
                TaxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType),
                AutopopulateDefaultTaxRate = autopopulateDefaultTaxRate,
                InvoiceTermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.TermsAndConditions),
                DefaultTaxRate = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.DefaultTaxRate),
                InvoiceTemplate = (InvoiceTemplateEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.InvoiceTemplate),
                QuickbooksIntegrationKind = (QuickbooksIntegrationKind)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.Quickbooks.IntegrationKind),
                QuickbooksInvoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix),
                IsQuickbooksConnected = await SettingManager.IsQuickbooksConnected(),
                QbdTaxAgencyVendorName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName),
                QbdDefaultIncomeAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName),
                QbdDefaultIncomeAccountType = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType),
                QbdAccountsReceivableAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName),
                QbdTaxAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAccountName),
                QbdIncomeAccountTypes = QuickbooksDesktop.Models.AccountTypes.GetIncomeTypesSelectList(),
            };
        }

        public async Task<QuoteSettingsEditDto> GetQuoteSettingsAsync()
        {
            var settings = new QuoteSettingsEditDto();
            settings.PromptForDisplayingQuarryInfoOnQuotes = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Quote.PromptForDisplayingQuarryInfoOnQuotes);
            settings.QuoteDefaultNote = await SettingManager.GetSettingValueAsync(AppSettings.Quote.DefaultNotes);
            settings.QuoteEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate);
            settings.QuoteEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailBodyTemplate);
            settings.QuoteChangedNotificationEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate);
            settings.QuoteChangedNotificationEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.BodyTemplate);
            settings.QuoteGeneralTermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Quote.GeneralTermsAndConditions);
            return settings;
        }

        private async Task<FuelSettingsEditDto> GetFuelSettingsAsync()
        {
            var settings = new FuelSettingsEditDto();
            settings.ShowFuelSurcharge = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            settings.ShowFuelSurchargeOnInvoice = (ShowFuelSurchargeOnInvoiceEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ShowFuelSurchargeOnInvoice);

            settings.ItemIdToUseForFuelSurchargeOnInvoice = await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            if (settings.ItemIdToUseForFuelSurchargeOnInvoice > 0)
            {
                var service = await _serviceRepository.GetAll()
                    .Select(x => new
                    {
                        x.Id,
                        x.Service1
                    })
                    .FirstOrDefaultAsync(x => x.Id == settings.ItemIdToUseForFuelSurchargeOnInvoice);
                if (service == null)
                {
                    settings.ItemIdToUseForFuelSurchargeOnInvoice = 0;
                }
                else
                {
                    settings.ItemNameToUseForFuelSurchargeOnInvoice = service.Service1;
                }
            }

            return settings;
        }

        private async Task<TenantOtherSettingsEditDto> GetOtherSettingsAsync()
        {
            return new TenantOtherSettingsEditDto()
            {
                IsQuickThemeSelectEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsQuickThemeSelectEnabled)
            };
        }

        private async Task<UserLockOutSettingsEditDto> GetUserLockOutSettingsAsync()
        {
            return new UserLockOutSettingsEditDto
            {
                IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled),
                MaxFailedAccessAttemptsBeforeLockout = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout),
                DefaultAccountLockoutSeconds = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds)
            };
        }

        private async Task<GpsIntegrationSettingsEditDto> GetGpsIntegrationSettingsAsync()
        {
            return new GpsIntegrationSettingsEditDto
            {
                Platform = (GpsPlatform)await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.Platform),
                Geotab = await GetGeotabSettingsAsync(),
                DtdTracker = await GetDtdTrackerSettingsAsync(),
                Samsara = await GetSamsaraSettingsAsync(),
                IntelliShift = await GetIntelliShiftSettingsAsync(),
            };
        }

        private async Task<GeotabSettingsEditDto> GetGeotabSettingsAsync()
        {
            return new GeotabSettingsEditDto
            {
                Server = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Server),
                Database = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Database),
                User = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.User),
                Password = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Password),
                MapBaseUrl = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.MapBaseUrl),
            };
        }
        private async Task<IntelliShiftSettingsEditDto> GetIntelliShiftSettingsAsync()
        {
            return new IntelliShiftSettingsEditDto
            {
                User = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.User),
                Password = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.Password),
            };
        }

        private async Task<SamsaraSettingsEditDto> GetSamsaraSettingsAsync()
        {
            return new SamsaraSettingsEditDto
            {
                ApiToken = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Samsara.ApiToken),
                BaseUrl = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Samsara.BaseUrl)
            };
        }

        private async Task<DtdTrackerSettingsEditDto> GetDtdTrackerSettingsAsync()
        {
            return new DtdTrackerSettingsEditDto
            {
                AccountName = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.DtdTracker.AccountName),
                AccountId = await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.AccountId),
            };
        }

        private async Task<DispatchingAndMessagingSettingsEditDto> GetDispatchingAndMessagingSettingsAsync()
        {
            var timezone = await GetTimezone();
            var result = new DispatchingAndMessagingSettingsEditDto
            {
                DispatchVia = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia),
                AllowSmsMessages = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSmsMessages),
                SendSmsOnDispatching = (SendSmsOnDispatchingEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.SendSmsOnDispatching),
                SmsPhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.SmsPhoneNumber),
                DriverDispatchSms = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate),
                DriverStartTime = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate),
                HideTicketControlsInDriverApp = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp),
                RequireDriversToEnterTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireDriversToEnterTickets),
                RequireSignature = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireSignature),
                RequireTicketPhoto = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireTicketPhoto),
                TextForSignatureView = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView),
                DispatchesLockedToTruck = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck),
                DefaultStartTime = (await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.DispatchingAndMessaging.DefaultStartTime)).ConvertTimeZoneTo(timezone),
                ShowTrailersOnSchedule = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule),
                ValidateUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization),
                AllowSchedulingTrucksWithoutDrivers = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers),
                AllowCounterSalesForTenant = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant),
            };

            return result;
        }

        private async Task<LeaseHaulerSettingsEditDto> GetLeaseHaulerSettingsEditDto()
        {
            return new LeaseHaulerSettingsEditDto
            {
                ShowLeaseHaulerRateOnQuote = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnQuote),
                ShowLeaseHaulerRateOnOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnOrder),
                AllowSubcontractorsToDriveCompanyOwnedTrucks = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks),
                BrokerFee = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.LeaseHaulers.BrokerFee),
                ThankYouForTrucksTemplate = await SettingManager.GetSettingValueAsync(AppSettings.LeaseHaulers.ThankYouForTrucksTemplate)
            };
        }

        private async Task<PaymentSettingsEditDto> GetPaymentSettingsAsync()
        {
            return new PaymentSettingsEditDto
            {
                PaymentProcessor = (PaymentProcessor)(await SettingManager.GetSettingValueAsync<int>(AppSettings.General.PaymentProcessor)),
                HeartlandPublicKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey),
                HeartlandSecretKey = SimpleStringCipher.Instance.Decrypt(await SettingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey)).IsNullOrEmpty()
                    ? string.Empty : DispatcherWebConsts.PasswordHasntBeenChanged
            };
        }

        private async Task<TruxSettingsEditDto> GetTruxSettingsAsync()
        {
            var truxSettings = new TruxSettingsEditDto
            {
                AllowImportingTruxEarnings = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Trux.AllowImportingTruxEarnings),
                UseForProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Trux.UseForProductionPay)
            };

            var customerId = await SettingManager.GetSettingValueAsync<int>(AppSettings.Trux.TruxCustomerId);
            if (customerId > 0)
            {
                var customer = await _customerRepository.GetAll()
                    .Where(x => x.Id == customerId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
                    }).FirstOrDefaultAsync();
                truxSettings.TruxCustomerId = customer?.Id;
                truxSettings.TruxCustomerName = customer?.Name;
            }

            if (truxSettings.TruxCustomerId == null)
            {
                var customers = await _customerRepository.GetAll()
                    .Where(x => x.Name.Contains("Trux"))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
                    })
                    .Take(2)
                    .ToListAsync();

                if (customers.Count == 1)
                {
                    truxSettings.TruxCustomerId = customers.First().Id;
                    truxSettings.TruxCustomerName = customers.First().Name;
                }
            }

            return truxSettings;
        }

        private async Task<LuckStoneSettingsEditDto> GetLuckStoneSettingsAsync()
        {
            var luckStoneSettings = new LuckStoneSettingsEditDto
            {
                AllowImportingLuckStoneEarnings = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LuckStone.AllowImportingLuckStoneEarnings),
                HaulerRef = await SettingManager.GetSettingValueAsync(AppSettings.LuckStone.HaulerRef),
                UseForProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LuckStone.UseForProductionPay)
            };

            var customerId = await SettingManager.GetSettingValueAsync<int>(AppSettings.LuckStone.LuckStoneCustomerId);
            if (customerId > 0)
            {
                var customer = await _customerRepository.GetAll()
                    .Where(x => x.Id == customerId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
                    }).FirstOrDefaultAsync();
                luckStoneSettings.LuckStoneCustomerId = customer?.Id;
                luckStoneSettings.LuckStoneCustomerName = customer?.Name;
            }

            if (luckStoneSettings.LuckStoneCustomerId == null)
            {
                var customers = await _customerRepository.GetAll()
                    .Where(x => x.Name.Contains("LuckStone") || x.Name.Contains("Luck Stone"))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
                    })
                    .Take(2)
                    .ToListAsync();

                if (customers.Count == 1)
                {
                    luckStoneSettings.LuckStoneCustomerId = customers.First().Id;
                    luckStoneSettings.LuckStoneCustomerName = customers.First().Name;
                }
            }

            return luckStoneSettings;
        }

        private Task<bool> IsTwoFactorLoginEnabledForApplicationAsync()
        {
            return SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled);
        }

        private async Task<TwoFactorLoginSettingsEditDto> GetTwoFactorLoginSettingsAsync()
        {
            var settings = new TwoFactorLoginSettingsEditDto
            {
                IsEnabledForApplication = await IsTwoFactorLoginEnabledForApplicationAsync()
            };

            if (_multiTenancyConfig.IsEnabled && !settings.IsEnabledForApplication)
            {
                return settings;
            }

            settings.IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled);
            settings.IsRememberBrowserEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled);

            if (!_multiTenancyConfig.IsEnabled)
            {
                settings.IsEmailProviderEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEmailProviderEnabled);
                settings.IsSmsProviderEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsSmsProviderEnabled);
                settings.IsGoogleAuthenticatorEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled);
            }

            return settings;
        }

        private async Task<bool> GetOneConcurrentLoginPerUserSetting()
        {
            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser);
        }

        private async Task<ExternalLoginProviderSettingsEditDto> GetExternalLoginProviderSettings()
        {
            var facebookSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Facebook, AbpSession.GetTenantId());
            var googleSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Google, AbpSession.GetTenantId());
            var twitterSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Twitter, AbpSession.GetTenantId());
            var microsoftSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Microsoft, AbpSession.GetTenantId());

            var openIdConnectSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect, AbpSession.GetTenantId());
            var openIdConnectMappedClaims = await SettingManager.GetSettingValueAsync(AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims);

            var wsFederationSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.WsFederation, AbpSession.GetTenantId());
            var wsFederationMappedClaims = await SettingManager.GetSettingValueAsync(AppSettings.ExternalLoginProvider.WsFederationMappedClaims);

            return new ExternalLoginProviderSettingsEditDto
            {
                Facebook_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated, AbpSession.GetTenantId()),
                Facebook = facebookSettings.IsNullOrWhiteSpace()
                    ? new FacebookExternalLoginProviderSettings()
                    : facebookSettings.FromJsonString<FacebookExternalLoginProviderSettings>(),

                Google_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated, AbpSession.GetTenantId()),
                Google = googleSettings.IsNullOrWhiteSpace()
                    ? new GoogleExternalLoginProviderSettings()
                    : googleSettings.FromJsonString<GoogleExternalLoginProviderSettings>(),

                Twitter_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated, AbpSession.GetTenantId()),
                Twitter = twitterSettings.IsNullOrWhiteSpace()
                    ? new TwitterExternalLoginProviderSettings()
                    : twitterSettings.FromJsonString<TwitterExternalLoginProviderSettings>(),

                Microsoft_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated, AbpSession.GetTenantId()),
                Microsoft = microsoftSettings.IsNullOrWhiteSpace()
                    ? new MicrosoftExternalLoginProviderSettings()
                    : microsoftSettings.FromJsonString<MicrosoftExternalLoginProviderSettings>(),

                OpenIdConnect_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated, AbpSession.GetTenantId()),
                OpenIdConnect = openIdConnectSettings.IsNullOrWhiteSpace()
                    ? new OpenIdConnectExternalLoginProviderSettings()
                    : openIdConnectSettings.FromJsonString<OpenIdConnectExternalLoginProviderSettings>(),
                OpenIdConnectClaimsMapping = openIdConnectMappedClaims.IsNullOrWhiteSpace()
                    ? new List<JsonClaimMapDto>()
                    : openIdConnectMappedClaims.FromJsonString<List<JsonClaimMapDto>>(),

                WsFederation_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated, AbpSession.GetTenantId()),
                WsFederation = wsFederationSettings.IsNullOrWhiteSpace()
                    ? new WsFederationExternalLoginProviderSettings()
                    : wsFederationSettings.FromJsonString<WsFederationExternalLoginProviderSettings>(),
                WsFederationClaimsMapping = wsFederationMappedClaims.IsNullOrWhiteSpace()
                    ? new List<JsonClaimMapDto>()
                    : wsFederationMappedClaims.FromJsonString<List<JsonClaimMapDto>>()
            };
        }
        #endregion

        #region Update Settings
        public async Task UpdateAllSettings(TenantSettingsEditDto input)
        {
            await UpdateUserManagementSettingsAsync(input.UserManagement);
            await UpdateSecuritySettingsAsync(input.Security);
            await UpdateBillingSettingsAsync(input.Billing);
            await UpdateEmailSettingsAsync(input.Email);
            await UpdateExternalLoginSettingsAsync(input.ExternalLoginProviderSettings);

            //Time Zone
            if (Clock.SupportsMultipleTimezone)
            {
                var oldTimezone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, AbpSession.GetTenantId());
                string newTimezone;
                if (input.General.Timezone.IsNullOrEmpty())
                {
                    var defaultValue = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Tenant, AbpSession.TenantId);
                    newTimezone = defaultValue;
                }
                else
                {
                    newTimezone = input.General.Timezone;
                }
                if (oldTimezone != newTimezone)
                {
                    await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), TimingSettingNames.TimeZone, newTimezone);
                    var offices = await _officeRepository.GetAll().ToListAsync();
                    foreach (var office in offices)
                    {
                        office.DefaultStartTime = office.DefaultStartTime?.ConvertTimeZoneTo(oldTimezone).ConvertTimeZoneFrom(newTimezone);
                    }
                }
            }

            if (!_multiTenancyConfig.IsEnabled)
            {
                await UpdateOtherSettingsAsync(input.OtherSettings);

                input.ValidateHostSettings();

                await UpdateEmailSettingsAsync(input.Email);

                if (_ldapModuleConfig.IsEnabled)
                {
                    await UpdateLdapSettingsAsync(input.Ldap);
                }
                await SettingManager.UpdateSmsSettingsAsync(input.Sms);

                await SettingManager.ChangeSettingForApplicationAsync(AppSettings.General.WebSiteRootAddress, input.General.WebSiteRootAddress);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                await UpdateGpsIntegrationSettingsAsync(input.GpsIntegration);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowPaymentProcessingFeature))
            {
                await UpdatePaymentSettingsAsync(input.Payment);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings))
            {
                await UpdateTruxSettingsAsync(input.Trux);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingLuckStoneEarnings))
            {
                await UpdateLuckStoneSettingsAsync(input.LuckStone);
            }

            if (await FeatureChecker.IsEnabledAsync(false, AppFeatures.GpsIntegrationFeature, AppFeatures.SmsIntegrationFeature, AppFeatures.DispatchingFeature))
            {
                await UpdateDispatchingAndMessagingSettingsAsync(input.DispatchingAndMessaging);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature))
            {
                await UpdateLeaseHaulerSettingsAsync(input.LeaseHaulers);
            }

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.DefaultNotes, input.Quote.QuoteDefaultNote);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.EmailSubjectTemplate, input.Quote.QuoteEmailSubjectTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.EmailBodyTemplate, input.Quote.QuoteEmailBodyTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate, input.Quote.QuoteChangedNotificationEmailSubjectTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.ChangedNotificationEmail.BodyTemplate, input.Quote.QuoteChangedNotificationEmailBodyTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.PromptForDisplayingQuarryInfoOnQuotes, input.Quote.PromptForDisplayingQuarryInfoOnQuotes.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Quote.GeneralTermsAndConditions, input.Quote.QuoteGeneralTermsAndConditions);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Order.EmailSubjectTemplate, input.General.OrderEmailSubjectTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Order.EmailBodyTemplate, input.General.OrderEmailBodyTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Receipt.EmailSubjectTemplate, input.General.ReceiptEmailSubjectTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Receipt.EmailBodyTemplate, input.General.ReceiptEmailBodyTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.CompanyName, input.General.CompanyName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.DefaultMapLocationAddress, input.General.DefaultMapLocationAddress);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.DefaultMapLocation, input.General.DefaultMapLocation);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.CurrencySymbol, input.General.CurrencySymbol);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.UserDefinedField1, input.General.UserDefinedField1);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.ValidateDriverAndTruckOnTickets, (!input.General.DontValidateDriverAndTruckOnTickets).ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.ShowDriverNamesOnPrintedOrder, input.General.ShowDriverNamesOnPrintedOrder.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.SplitBillingByOffices, input.General.SplitBillingByOffices.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders, input.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.DriverOrderNotification.EmailTitle, input.General.DriverOrderEmailTitle);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.DriverOrderNotification.EmailBody, input.General.DriverOrderEmailBody);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.DriverOrderNotification.Sms, input.General.DriverOrderSms);

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.UseShifts, input.General.UseShifts.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.ShiftName1, input.General.ShiftName1);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.ShiftName2, input.General.ShiftName2);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.ShiftName3, input.General.ShiftName3);

            var defaultJob = await _timeClassificationRepository.GetAll()
                    .Select(x => new
                    {
                        x.Id,
                        x.IsProductionBased
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.TimeAndPay.TimeTrackingDefaultTimeClassificationId);

            if (defaultJob == null)
            {
                throw new UserFriendlyException(L("DefaultJobIsRequired"));
            }
            if (!input.TimeAndPay.AllowProductionPay)
            {
                if (defaultJob.IsProductionBased)
                {
                    throw new UserFriendlyException(L("CannotDisallowProductionPayBecauseOfDefaultJob"));
                }
                //var hasEmployeeTimeClassifications = await _employeeTimeClassificationRepository.GetAll()
                //    .Where(x => x.TimeClassification.IsProductionBased)
                //    .AnyAsync();
                //if (hasEmployeeTimeClassifications)
                //{
                //    throw new UserFriendlyException(L("CannotDisallowProductionPayBecauseOfDrivers"));
                //}
            }
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            if (!allowProductionPay && input.TimeAndPay.AllowProductionPay)
            {
                var driversWithMissingClassifications = await _driverRepository.GetAll()
                    .Where(x => x.OfficeId != null && !x.EmployeeTimeClassifications.Any(t => t.TimeClassification.IsProductionBased))
                    .Select(x => x.Id)
                    .ToListAsync();

                var productionPay = await _timeClassificationRepository.GetAll()
                    .Where(x => x.IsProductionBased)
                    .FirstOrDefaultAsync();

                if (productionPay == null)
                {
                    productionPay = new TimeClassification { TenantId = AbpSession.GetTenantId(), Name = "Production Pay", IsProductionBased = true };
                    await _timeClassificationRepository.InsertAndGetIdAsync(productionPay);
                }

                foreach (var driverId in driversWithMissingClassifications)
                {
                    await _employeeTimeClassificationRepository.InsertAsync(new EmployeeTimeClassification
                    {
                        DriverId = driverId,
                        TimeClassificationId = productionPay.Id
                    });
                }
            }
            else if (allowProductionPay && !input.TimeAndPay.AllowProductionPay)
            {
                var today = await GetToday();
                var orderLines = await _orderLineRepository.GetAll().Where(x => x.Order.DeliveryDate >= today && x.ProductionPay).ToListAsync();
                orderLines.ForEach(x => x.ProductionPay = false);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, input.TimeAndPay.TimeTrackingDefaultTimeClassificationId.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TimeAndPay.AllowProductionPay, input.TimeAndPay.AllowProductionPay.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TimeAndPay.DefaultToProductionPay, input.TimeAndPay.DefaultToProductionPay.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate, input.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs, input.TimeAndPay.PreventProductionPayOnHourlyJobs.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TimeAndPay.DriverIsPaidForLoadBasedOn, input.TimeAndPay.DriverIsPaidForLoadBasedOn.ToIntString());

            await UpdateFuelSettingsAsync(input.Fuel);
        }

        private async Task UpdateOtherSettingsAsync(TenantOtherSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.IsQuickThemeSelectEnabled,
                input.IsQuickThemeSelectEnabled.ToString().ToLowerInvariant()
            );
        }
        private async Task UpdateBillingSettingsAsync(TenantBillingSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TenantManagement.BillingLegalName, input.LegalName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TenantManagement.BillingAddress, input.Address);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TenantManagement.BillingPhoneNumber, input.PhoneNumber);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.RemitToInformation, input.RemitToInformation);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.DefaultMessageOnInvoice, input.DefaultMessageOnInvoice);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.EmailSubjectTemplate, input.InvoiceEmailSubjectTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.EmailBodyTemplate, input.InvoiceEmailBodyTemplate);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.TenantManagement.BillingTaxVatNo, input.TaxVatNo);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.TaxCalculationType, ((int)input.TaxCalculationType).ToString("N0"));
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.AutopopulateDefaultTaxRate, (input.TaxCalculationType == TaxCalculationType.NoCalculation ? false : input.AutopopulateDefaultTaxRate).ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.TermsAndConditions, input.InvoiceTermsAndConditions);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.DefaultTaxRate, (input.DefaultTaxRate ?? 0).ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.InvoiceTemplate, ((int)input.InvoiceTemplate).ToString("N0"));
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.Quickbooks.IntegrationKind, ((int)(input.QuickbooksIntegrationKind ?? 0)).ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix, input.QuickbooksInvoiceNumberPrefix);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName, input.QbdTaxAgencyVendorName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName, input.QbdDefaultIncomeAccountName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType, input.QbdDefaultIncomeAccountType);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName, input.QbdAccountsReceivableAccountName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Invoice.QuickbooksDesktop.TaxAccountName, input.QbdTaxAccountName);
        }

        private async Task UpdateLdapSettingsAsync(LdapSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), LdapSettingNames.IsEnabled, input.IsEnabled.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), LdapSettingNames.Domain, input.Domain.IsNullOrWhiteSpace() ? null : input.Domain);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), LdapSettingNames.UserName, input.UserName.IsNullOrWhiteSpace() ? null : input.UserName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), LdapSettingNames.Password, input.Password.IsNullOrWhiteSpace() ? null : input.Password);
        }

        private async Task UpdateEmailSettingsAsync(TenantEmailSettingsEditDto input)
        {
            if (_multiTenancyConfig.IsEnabled && !DispatcherWebConsts.AllowTenantsToChangeEmailSettings)
            {
                return;
            }
            var useHostDefaultEmailSettings = _multiTenancyConfig.IsEnabled && input.UseHostDefaultEmailSettings;

            if (useHostDefaultEmailSettings)
            {
                var smtpPassword = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.Password);

                input = new TenantEmailSettingsEditDto
                {
                    UseHostDefaultEmailSettings = true,
                    DefaultFromAddress = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.DefaultFromAddress),
                    DefaultFromDisplayName = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.DefaultFromDisplayName),
                    SmtpHost = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.Host),
                    SmtpPort = await SettingManager.GetSettingValueForApplicationAsync<int>(EmailSettingNames.Smtp.Port),
                    SmtpUserName = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.UserName),
                    SmtpPassword = SimpleStringCipher.Instance.Decrypt(smtpPassword),
                    SmtpDomain = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.Domain),
                    SmtpEnableSsl = await SettingManager.GetSettingValueForApplicationAsync<bool>(EmailSettingNames.Smtp.EnableSsl),
                    SmtpUseDefaultCredentials = await SettingManager.GetSettingValueForApplicationAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials)
                };
            }

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Email.UseHostDefaultEmailSettings, useHostDefaultEmailSettings.ToString().ToLowerInvariant());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.DefaultFromAddress, input.DefaultFromAddress);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.DefaultFromDisplayName, input.DefaultFromDisplayName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.Host, input.SmtpHost);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.Port, input.SmtpPort.ToString(CultureInfo.InvariantCulture));
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.UserName, input.SmtpUserName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.Password, SimpleStringCipher.Instance.Encrypt(input.SmtpPassword));
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.Domain, input.SmtpDomain);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.EnableSsl, input.SmtpEnableSsl.ToString().ToLowerInvariant());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), EmailSettingNames.Smtp.UseDefaultCredentials, input.SmtpUseDefaultCredentials.ToString().ToLowerInvariant());
        }

        private async Task UpdateUserManagementSettingsAsync(TenantUserManagementSettingsEditDto settings)
        {
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.AllowSelfRegistration,
                settings.AllowSelfRegistration.ToLowerCaseString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault,
                settings.IsNewRegisteredUserActiveByDefault.ToLowerCaseString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin,
                settings.IsEmailConfirmationRequiredForLogin.ToLowerCaseString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.UseCaptchaOnRegistration,
                settings.UseCaptchaOnRegistration.ToLowerCaseString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.UseCaptchaOnLogin,
                settings.UseCaptchaOnLogin.ToString().ToLowerInvariant()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.IsCookieConsentEnabled,
                settings.IsCookieConsentEnabled.ToLowerCaseString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.AllowUsingGravatarProfilePicture,
                settings.AllowUsingGravatarProfilePicture.ToString().ToLowerInvariant()
            );

            await UpdateUserManagementSessionTimeOutSettingsAsync(settings.SessionTimeOutSettings);
        }

        private async Task UpdateUserManagementSessionTimeOutSettingsAsync(SessionTimeOutSettingsEditDto settings)
        {
            if (settings == null)
            {
                return;
            }

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.SessionTimeOut.IsEnabled,
                settings.IsEnabled.ToString().ToLowerInvariant()
            );
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.SessionTimeOut.TimeOutSecond,
                settings.TimeOutSecond.ToString()
            );
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond,
                settings.ShowTimeOutNotificationSecond.ToString()
            );
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut,
                settings.ShowLockScreenWhenTimedOut.ToString()
            );
        }

        private async Task UpdateSecuritySettingsAsync(SecuritySettingsEditDto settings)
        {
            if (settings.UseDefaultPasswordComplexitySettings)
            {
                await UpdatePasswordComplexitySettingsAsync(settings.DefaultPasswordComplexity);
            }
            else
            {
                await UpdatePasswordComplexitySettingsAsync(settings.PasswordComplexity);
            }

            await UpdateUserLockOutSettingsAsync(settings.UserLockOut);
            await UpdateTwoFactorLoginSettingsAsync(settings.TwoFactorLogin);
            await UpdateOneConcurrentLoginPerUserSettingAsync(settings.AllowOneConcurrentLoginPerUser);
        }

        private async Task UpdatePasswordComplexitySettingsAsync(PasswordComplexitySetting settings)
        {
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit,
                settings.RequireDigit.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase,
                settings.RequireLowercase.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric,
                settings.RequireNonAlphanumeric.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase,
                settings.RequireUppercase.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength,
                settings.RequiredLength.ToString()
            );
        }

        private async Task UpdateUserLockOutSettingsAsync(UserLockOutSettingsEditDto settings)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled, settings.IsEnabled.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds, settings.DefaultAccountLockoutSeconds.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout, settings.MaxFailedAccessAttemptsBeforeLockout.ToString());
        }

        private async Task UpdateTwoFactorLoginSettingsAsync(TwoFactorLoginSettingsEditDto settings)
        {
            if (_multiTenancyConfig.IsEnabled &&
                !await IsTwoFactorLoginEnabledForApplicationAsync()) //Two factor login can not be used by tenants if disabled by the host
            {
                return;
            }

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled, settings.IsEnabled.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled, settings.IsRememberBrowserEnabled.ToLowerCaseString());

            if (!_multiTenancyConfig.IsEnabled)
            {
                //These settings can only be changed by host, in a multitenant application.
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEmailProviderEnabled, settings.IsEmailProviderEnabled.ToLowerCaseString());
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsSmsProviderEnabled, settings.IsSmsProviderEnabled.ToLowerCaseString());
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled, settings.IsGoogleAuthenticatorEnabled.ToLowerCaseString());
            }
        }


        private async Task UpdateGpsIntegrationSettingsAsync(GpsIntegrationSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Platform, input.Platform.ToIntString());

            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.DtdTracker.AccountName, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.Server, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.Database, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.User, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.Password, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.MapBaseUrl, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Samsara.ApiToken, null);
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Samsara.BaseUrl, null);

            if (input.Platform == GpsPlatform.DtdTracker)
            {
                //await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.DtdTracker.AccountName, input.DtdTracker.AccountName);
            }
            else if (input.Platform == GpsPlatform.Geotab)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.Server, input.Geotab.Server);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.Database, input.Geotab.Database);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.User, input.Geotab.User);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.Password, input.Geotab.Password);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Geotab.MapBaseUrl, input.Geotab.MapBaseUrl);
            }
            else if (input.Platform == GpsPlatform.Samsara)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Samsara.ApiToken, input.Samsara.ApiToken);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.Samsara.BaseUrl, input.Samsara.BaseUrl);
            }
            else if (input.Platform == GpsPlatform.IntelliShift)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.IntelliShift.User, input.IntelliShift.User);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.IntelliShift.Password, input.IntelliShift.Password);
            }
        }

        private async Task UpdateDispatchingAndMessagingSettingsAsync(DispatchingAndMessagingSettingsEditDto input)
        {
            var timezone = await GetTimezone();
            var allowCounterSalesForTenant = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant);
            var allowCounterSalesForTenantWasUnchecked = allowCounterSalesForTenant && !input.AllowCounterSalesForTenant
                && await _settingAvailabilityProvider.IsSettingAvailableAsync(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant);

            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.DispatchVia, input.DispatchVia.ToIntString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.AllowSmsMessages, input.AllowSmsMessages.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.SendSmsOnDispatching, input.SendSmsOnDispatching.ToIntString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.SmsPhoneNumber, input.SmsPhoneNumber);
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate, input.DriverDispatchSms);
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate, input.DriverStartTime);
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp, input.HideTicketControlsInDriverApp.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.RequireDriversToEnterTickets, input.RequireDriversToEnterTickets.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.RequireSignature, input.RequireSignature.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.RequireTicketPhoto, input.RequireTicketPhoto.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView, input.TextForSignatureView);
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck, input.DispatchesLockedToTruck.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.DefaultStartTime, input.DefaultStartTime.ConvertTimeZoneFrom(timezone).ToString("s"));
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule, input.ShowTrailersOnSchedule.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers, input.AllowSchedulingTrucksWithoutDrivers.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.ValidateUtilization, input.ValidateUtilization.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant, input.AllowCounterSalesForTenant.ToLowerCaseString());
            
            if (allowCounterSalesForTenantWasUnchecked)
            {
                var userSettingsToReset = new[]
                {
                    AppSettings.DispatchingAndMessaging.AllowCounterSalesForUser,
                    AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly,
                    AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId,
                    AppSettings.DispatchingAndMessaging.DefaultServiceId,
                    AppSettings.DispatchingAndMessaging.DefaultMaterialUomId,
                    AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber,
                };
                var userIds = await UserManager.Users.Select(x => x.Id).ToListAsync();
                foreach (var userId in userIds)
                {
                    var userIdentifier = new UserIdentifier(AbpSession.GetTenantId(), userId);
                    foreach (var settingToReset in userSettingsToReset)
                    {
                        var defaultValue = _settingDefinitionManager.GetSettingDefinition(settingToReset).DefaultValue;
                        await SettingManager.ChangeSettingForUserAsync(userIdentifier, settingToReset, defaultValue);
                    }
                }
            }
        }

        private async Task ChangeSettingForTenantIfAvailableAsync(string settingName, string newValue)
        {
            if (await _settingAvailabilityProvider.IsSettingAvailableAsync(settingName))
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), settingName, newValue);
            }
        }

        private async Task UpdateLeaseHaulerSettingsAsync(LeaseHaulerSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnQuote, input.ShowLeaseHaulerRateOnQuote.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnOrder, input.ShowLeaseHaulerRateOnOrder.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks, input.AllowSubcontractorsToDriveCompanyOwnedTrucks.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LeaseHaulers.BrokerFee, input.BrokerFee.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LeaseHaulers.ThankYouForTrucksTemplate, input.ThankYouForTrucksTemplate);
        }

        private async Task UpdatePaymentSettingsAsync(PaymentSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.General.PaymentProcessor, ((int)input.PaymentProcessor).ToString("N0"));
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Heartland.PublicKey, input.HeartlandPublicKey);
            if (input.HeartlandSecretKey != DispatcherWebConsts.PasswordHasntBeenChanged)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Heartland.SecretKey, SimpleStringCipher.Instance.Encrypt(input.HeartlandSecretKey));
            }
        }

        private async Task UpdateTruxSettingsAsync(TruxSettingsEditDto input)
        {
            if (input.AllowImportingTruxEarnings && !(input.TruxCustomerId > 0))
            {
                throw new UserFriendlyException("Trux Customer is required");
            }
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Trux.AllowImportingTruxEarnings, input.AllowImportingTruxEarnings.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Trux.UseForProductionPay, input.UseForProductionPay.ToLowerCaseString());
            if (input.AllowImportingTruxEarnings)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Trux.TruxCustomerId, input.TruxCustomerId.ToString());
            }
        }

        private async Task UpdateLuckStoneSettingsAsync(LuckStoneSettingsEditDto input)
        {
            if (input.AllowImportingLuckStoneEarnings)
            {
                if (!(input.LuckStoneCustomerId > 0))
                {
                    throw new UserFriendlyException("Luck Stone Customer is required");
                }
                if (string.IsNullOrEmpty(input.HaulerRef))
                {
                    throw new UserFriendlyException("HaulerRef is required");
                }
            }
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LuckStone.AllowImportingLuckStoneEarnings, input.AllowImportingLuckStoneEarnings.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LuckStone.UseForProductionPay, input.UseForProductionPay.ToLowerCaseString());
            if (input.AllowImportingLuckStoneEarnings)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LuckStone.LuckStoneCustomerId, input.LuckStoneCustomerId.ToString());
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.LuckStone.HaulerRef, input.HaulerRef);
            }
        }

        private async Task UpdateFuelSettingsAsync(FuelSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Fuel.ShowFuelSurcharge, input.ShowFuelSurcharge.ToLowerCaseString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Fuel.ShowFuelSurchargeOnInvoice, input.ShowFuelSurchargeOnInvoice.ToIntString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice, (input.ItemIdToUseForFuelSurchargeOnInvoice ?? 0).ToString());
        }

        private async Task UpdateOneConcurrentLoginPerUserSettingAsync(bool allowOneConcurrentLoginPerUser)
        {
            if (_multiTenancyConfig.IsEnabled)
            {
                return;
            }
            await SettingManager.ChangeSettingForApplicationAsync(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser, allowOneConcurrentLoginPerUser.ToString());
        }

        private async Task UpdateExternalLoginSettingsAsync(ExternalLoginProviderSettingsEditDto input)
        {
            if (input == null)
            {
                return;
            }

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Facebook,
                input.Facebook == null || !input.Facebook.IsValid() ? "" : input.Facebook.ToJsonString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated,
                input.Facebook_IsDeactivated.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Google,
                input.Google == null || !input.Google.IsValid() ? "" : input.Google.ToJsonString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated,
                input.Google_IsDeactivated.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Twitter,
                input.Twitter == null || !input.Twitter.IsValid() ? "" : input.Twitter.ToJsonString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated,
                input.Twitter_IsDeactivated.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Microsoft,
                input.Microsoft == null || !input.Microsoft.IsValid() ? "" : input.Microsoft.ToJsonString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated,
                input.Microsoft_IsDeactivated.ToString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect,
                input.OpenIdConnect == null || !input.OpenIdConnect.IsValid() ? "" : input.OpenIdConnect.ToJsonString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated,
                input.OpenIdConnect_IsDeactivated.ToString()
            );

            var openIdConnectMappedClaimsValue = "";
            if (input.OpenIdConnect == null || !input.OpenIdConnect.IsValid() || input.OpenIdConnectClaimsMapping.IsNullOrEmpty())
            {
                openIdConnectMappedClaimsValue = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims);//set default value
            }
            else
            {
                openIdConnectMappedClaimsValue = input.OpenIdConnectClaimsMapping.ToJsonString();
            }

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims,
                openIdConnectMappedClaimsValue
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.WsFederation,
                input.WsFederation == null || !input.WsFederation.IsValid() ? "" : input.WsFederation.ToJsonString()
            );

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated,
                input.WsFederation_IsDeactivated.ToString()
            );

            var wsFederationMappedClaimsValue = "";
            if (input.WsFederation == null || !input.WsFederation.IsValid() || input.WsFederationClaimsMapping.IsNullOrEmpty())
            {
                wsFederationMappedClaimsValue = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.WsFederationMappedClaims);//set default value
            }
            else
            {
                wsFederationMappedClaimsValue = input.WsFederationClaimsMapping.ToJsonString();
            }

            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.ExternalLoginProvider.WsFederationMappedClaims,
                wsFederationMappedClaimsValue
            );

            ExternalLoginOptionsCacheManager.ClearCache();
        }

        #endregion

        #region Others

        public async Task ClearLogo()
        {
            var tenant = await GetCurrentTenantAsync();

            if (!tenant.HasLogo())
            {
                return;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.LogoId.Value);
            if (logoObject != null)
            {
                await _binaryObjectManager.DeleteAsync(tenant.LogoId.Value);
            }

            tenant.ClearLogo();
        }

        public async Task ClearReportsLogo()
        {
            var tenant = await GetCurrentTenantAsync();

            if (tenant.ReportsLogoId == null)
            {
                return;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.ReportsLogoId.Value);
            if (logoObject != null)
            {
                await _binaryObjectManager.DeleteAsync(tenant.ReportsLogoId.Value);
            }

            tenant.ReportsLogoId = null;
            tenant.ReportsLogoFileType = null;
        }

        public async Task ClearCustomCss()
        {
            var tenant = await GetCurrentTenantAsync();

            if (!tenant.CustomCssId.HasValue)
            {
                return;
            }

            var cssObject = await _binaryObjectManager.GetOrNullAsync(tenant.CustomCssId.Value);
            if (cssObject != null)
            {
                await _binaryObjectManager.DeleteAsync(tenant.CustomCssId.Value);
            }

            tenant.CustomCssId = null;
        }

        public string GetDefaultDriverDispatchSmsTemplate() =>
            _settingDefinitionManager.GetSettingDefinition(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate).DefaultValue;

        #endregion

        public async Task<bool> CanLinkDtdTrackerAccount()
        {
            var platform = await SettingManager.GetGpsPlatformAsync();
            var dtdTrackerAccountId = await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.AccountId);
            return platform == GpsPlatform.DtdTracker && dtdTrackerAccountId == 0;
        }

        public async Task LinkDtdTrackerAccount(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            var account = await _dtdTrackerTelematics.GetAccountDetailsFromAccessToken(accessToken);
            if (!account.AccountName.IsNullOrEmpty() && account.AccountId > 0)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.DtdTracker.AccountName, account.AccountName);
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.DtdTracker.AccountId, account.AccountId.ToString());
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AppSettings.GpsIntegration.DtdTracker.UserId, account.UserId.ToString());
            }
            else
            {
                Logger.Error("Received unexpected DTDTracker account " + Newtonsoft.Json.JsonConvert.SerializeObject(account));
            }
        }
    }
}
