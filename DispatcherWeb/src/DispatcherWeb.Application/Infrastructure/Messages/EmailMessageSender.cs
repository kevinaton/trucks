using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.Notifications;

namespace DispatcherWeb.Infrastructure.Messages
{
    public class EmailMessageSender : ITransientDependency, IEmailMessageSender
    {
        private readonly IEmailSender _emailSender;
        private readonly IAppNotifier _appNotifier;
        private readonly IAbpSession _abpSession;

        public ILogger Logger { get; set; }

        public EmailMessageSender(
            IEmailSender emailSender,
            IAppNotifier appNotifier,
            IAbpSession abpSession
        )
        {
            Logger = NullLogger.Instance;
            _emailSender = emailSender;
            _appNotifier = appNotifier;
            _abpSession = abpSession;
        }

        public async Task<bool> SendEmailMessageAndNotifyErrors(string fromEmailAddress, string toEmailAddress, string subject, string messageBody, string recipientName)
        {
            var message = new MailMessage(fromEmailAddress, toEmailAddress)
            {
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = false
            };
            try
            {
                await _emailSender.SendAsync(message);
            }
            catch (Exception e)
            {
                await _appNotifier.SendMessageAsync(
                    _abpSession.ToUserIdentifier(),
                    $"Unable to send the message to {recipientName}. Details: {e.Message}",
                    Abp.Notifications.NotificationSeverity.Error
                );
                return false;
            }

            return true;
        }


    }
}
