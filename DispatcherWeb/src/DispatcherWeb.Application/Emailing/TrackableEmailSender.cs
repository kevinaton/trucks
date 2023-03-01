using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Net.Mail.Smtp;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace DispatcherWeb.Emailing
{
    public class TrackableEmailSender : DispatcherWebEmailSender, ITrackableEmailSender, ITransientDependency
    {
        private readonly IEmailAppService _emailAppService;
        private readonly IWebUrlService _webUrlService;
        private readonly AspNetZeroAbpSession _session;

        public TrackableEmailSender(
            ISmtpEmailSenderConfiguration configuration,
            IWebHostEnvironment env,
            IEmailAppService emailAppService,
            IWebUrlService webUrlService,
            AspNetZeroAbpSession session
            )
            : base(configuration, env)
        {
            _emailAppService = emailAppService;
            _webUrlService = webUrlService;
            _session = session;
        }

        public Guid SendTrackable(MailMessage mail, bool normalize = true)
        {
            if (normalize)
                NormalizeMail(mail);

            var trackableEmailId = _emailAppService.AddTrackableEmail(mail);
            AppendSendGridApiHeader(mail, trackableEmailId);

            SendEmail(mail);

            return trackableEmailId;
        }

        public async Task<Guid> SendTrackableAsync(MailMessage mail, bool normalize = true)
        {
            if (normalize)
                NormalizeMail(mail);

            var trackableEmailId = await _emailAppService.AddTrackableEmailAsync(mail);
            AppendSendGridApiHeader(mail, trackableEmailId);

            await SendEmailAsync(mail);

            return trackableEmailId;
        }

        private void AppendSendGridApiHeader(MailMessage mail, Guid trackableEmailId)
        {
            var receivers = mail.GetTrackableEmailReceivers(trackableEmailId);
            var receiverEmails = receivers.Select(x => x.Email).ToList();

            var callbackUrl = _webUrlService.GetSiteRootAddress().EnsureEndsWith('/')
                              + "/app/Emails/TrackEvents";

            var apiMessage = new
            {
                to = receiverEmails,
                sub = new
                {
                    __email__ = receiverEmails
                },
                unique_args = new
                {
                    trackableEmailId = trackableEmailId,
                    trackableEmailCallbackUrl = callbackUrl,
                    trackableEmailTenantId = _session.TenantId
                }
            };

            mail.Headers.Add("X-SMTPAPI", JsonConvert.SerializeObject(apiMessage));

            if (!mail.IsBodyHtml)
            {
                mail.Body = WebUtility.HtmlDecode(mail.Body);
                mail.Body = mail.Body
                    .Replace("\n", "\n<br>");
                mail.IsBodyHtml = true;
            }

            var trackOpenUrl = _webUrlService.GetSiteRootAddress().EnsureEndsWith('/')
                               + $"app/Emails/TrackEmailOpen/{trackableEmailId}?email=__email__";
            mail.Body += "<img src=\"" + trackOpenUrl + "\" height=\"1\" width=\"1\" />";
        }
    }
}
