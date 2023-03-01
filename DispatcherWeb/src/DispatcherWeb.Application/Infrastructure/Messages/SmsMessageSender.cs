using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Notifications;
using Twilio.Exceptions;

namespace DispatcherWeb.Infrastructure.Messages
{
    public class SmsMessageSender : ITransientDependency, ISmsMessageSender
    {
        private readonly ISmsSender _smsSender;
        private readonly IAppNotifier _appNotifier;
        private readonly IAbpSession _abpSession;

        public ILogger Logger { get; set; }

        public SmsMessageSender(
            ISmsSender smsSender,
            IAppNotifier appNotifier,
            IAbpSession abpSession
        )
        {
            Logger = NullLogger.Instance;
            _smsSender = smsSender;
            _appNotifier = appNotifier;
            _abpSession = abpSession;
        }

        public async Task<bool> SendSmsMessageAndNotifyErrors(string cellPhoneNumber, string messageBody, string recipientName)
        {
            var sendSmsResult = await SendSms(cellPhoneNumber, messageBody);
            switch (sendSmsResult)
            {
                case SendSmsResultError e:
                    await _appNotifier.SendMessageAsync(
                        _abpSession.ToUserIdentifier(),
                        $"Unable to send the message to {recipientName}. Details: {e.ErrorMessage}",
                        Abp.Notifications.NotificationSeverity.Error
                    );
                    return false;
                case SendSmsResultSuccess _:
                    break;
            }

            return true;
        }

        private async Task<SendSmsResult> SendSms(string cellPhoneNumber, string body)
        {
            string smsText = $"{body}";
            try
            {
                await _smsSender.SendAsync(smsText, cellPhoneNumber, true);
            }
            catch (ApiException e)
            {
                Logger.Error(e.ToString);
                return new SendSmsResultError(e.Message);
            }

            return new SendSmsResultSuccess();


        }
    }
}