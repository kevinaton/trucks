using System.Net.Mail;
using DispatcherWeb.Debugging;
using Abp.Net.Mail.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Emailing
{
    public class DispatcherWebEmailSender : SmtpEmailSender
    {
        private readonly IWebHostEnvironment _env;

        public DispatcherWebEmailSender(ISmtpEmailSenderConfiguration configuration, IWebHostEnvironment env)
            : base(configuration)
        {
            _env = env;
        }

        protected override void NormalizeMail(MailMessage mail)
        {
            base.NormalizeMail(mail);

            if (!_env.IsProduction())
            {
                mail.Subject = $"{_env.EnvironmentName}{(DebugHelper.IsDebug ? " Debug" : "")}: {mail.Subject}";
            }
        }
    }
}
