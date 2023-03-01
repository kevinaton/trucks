using System;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Localization;
using DispatcherWeb.Sms;
using DispatcherWeb.Url;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace DispatcherWeb.Infrastructure.Sms
{
    public class SmsSender : ISmsSender, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IWebUrlService _webUrlService;
        private readonly IRepository<SentSms> _sentSmsRepository;
        private readonly LocalizationHelper _localizationHelper;
        private string _accountSid;
        private string _authToken;
        private string _phoneNumber;

        public ILogger Logger { get; set; }
        public IAbpSession AbpSession { get; set; }

        public SmsSender(
            ISettingManager settingManager,
            IWebUrlService webUrlService,
            IRepository<SentSms> sentSmsRepository,
            LocalizationHelper localizationHelper
        )
        {
            _settingManager = settingManager;
            _webUrlService = webUrlService;
            _sentSmsRepository = sentSmsRepository;
            _localizationHelper = localizationHelper;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        private void ReadSettings(bool useTenantPhoneNumberOnly)
        {
            _accountSid = _settingManager.GetSettingValue(AppSettings.Sms.AccountSid);
            _authToken = _settingManager.GetSettingValue(AppSettings.Sms.AuthToken);
            _phoneNumber = AbpSession.TenantId.HasValue ? _settingManager.GetSettingValueForTenant(AppSettings.DispatchingAndMessaging.SmsPhoneNumber, AbpSession.TenantId.Value) : null;
            if (!useTenantPhoneNumberOnly && _phoneNumber.IsNullOrEmpty())
            {
                _phoneNumber = _settingManager.GetSettingValue(AppSettings.Sms.PhoneNumber);
            }
            if (_accountSid.IsNullOrEmpty() || _authToken.IsNullOrEmpty() || _phoneNumber.IsNullOrEmpty())
            {
                throw new UserFriendlyException("There are no SMS settings. Please contact your administrator.");
            }
        }

        public async Task<SmsSendResult> SendAsync(string text, string toPhoneNumber, bool trackStatus = false, bool useTenantPhoneNumberOnly = false, bool insertEntity = true)
        {
            if (text?.Length > AppConsts.MaxSmsLength)
            {
                throw new ApiException(_localizationHelper.L("SmsLengthLimitOf{0}Exceeded", AppConsts.MaxSmsLength));
            }

            ReadSettings(useTenantPhoneNumberOnly);

            Logger.Debug($"Sending a message with text: '{text}' to number: '{toPhoneNumber}' from number: '{_phoneNumber}'");
            TwilioClient.Init(_accountSid, _authToken);

            Uri callbackUri = null;
            if (trackStatus)
            {
                string siteUrl = _webUrlService.GetSiteRootAddress();
                callbackUri = siteUrl.Contains("://localhost")
                    ? new Uri("https://postb.in/hB8zxLkm") // Localhost. Use https://postb.in/b/hB8zxLkm to check
                    : new Uri($"{siteUrl}app/SmsCallback");
            }

            MessageResource message = null;
            SentSms sentSms;
            bool sentSmsEntityIsInserted = false;
            try
            {
                message = await MessageResource.CreateAsync(
                    body: text,
                    from: new PhoneNumber(_phoneNumber),
                    to: new PhoneNumber(toPhoneNumber),
                    statusCallback: callbackUri
                );
#if DEBUG
                var smsLogger = Logger.CreateChildLogger("SmsLogger");
                smsLogger.Info($"SMS message:\n Body: {text}\n From: {_phoneNumber}\n To: {toPhoneNumber}\n");
#endif
                sentSms = new SentSms
                {
                    FromSmsNumber = _phoneNumber,
                    ToSmsNumber = toPhoneNumber,
                    TenantId = AbpSession.TenantId,
                    Sid = message.Sid,
                    Status = message.Status.ToSmsStatus()
                };

                if (trackStatus)
                {
                    sentSmsEntityIsInserted = true;
                    await _sentSmsRepository.InsertAndGetIdAsync(sentSms);
                }
                else if (insertEntity)
                {
                    sentSmsEntityIsInserted = true;
                    await _sentSmsRepository.InsertAsync(sentSms);
                }
            }
            catch (ApiException e)
            {
                Logger.Error($"Exception when sending the sms: {e.Message}");
                throw;
            }

            if (message.ErrorCode.HasValue)
            {
                sentSms.Status = SmsStatus.Failed;
                Logger.Error($"There was an error: '{message.ErrorMessage}' while sending the sms to the number: '{toPhoneNumber}' with the text: '{text}'");
            }
            else
            {
                Logger.Debug($"The sms to the number: '{toPhoneNumber}' with the text: '{text}' was sent");
            }

            return new SmsSendResult
            {
                Sid = message.Sid,
                Status = message.Status.ToSmsStatus(),
                ErrorCode = message.ErrorCode,
                ErrorMessage = message.ErrorMessage,
                SentSmsEntity = sentSms,
                SentSmsEntityIsInserted = sentSmsEntityIsInserted
            };
        }
    }
}
