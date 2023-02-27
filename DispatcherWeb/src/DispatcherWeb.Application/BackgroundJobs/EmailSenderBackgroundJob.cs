using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Net.Mail;
using Abp.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class EmailSenderBackgroundJob : AsyncBackgroundJob<EmailSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IAbpSession _abpSession;
        private readonly IEmailSender _emailSender;

        public EmailSenderBackgroundJob(
            IAbpSession abpSession,
            IEmailSender emailSender
            )
        {
            _abpSession = abpSession;
            _emailSender = emailSender;
        }

        public override async Task ExecuteAsync(EmailSenderBackgroundJobArgs args)
        {
            using (_abpSession.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            {
                await SendEmailBatches(args.EmailInputs);
            }
        }

        private async Task SendEmailBatches(List<EmailSenderBackgroundJobArgsEmail> inputs)
        {
            foreach (var input in inputs)
            {
                try
                {
                    await _emailSender.SendAsync(input.ToEmailAddress, input.Subject, input.Text, false);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error during batch Email sending", ex);
                }
            }
        }
    }
}
