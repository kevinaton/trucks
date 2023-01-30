using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DispatcherWeb.Emailing
{
    public interface ITrackableEmailSender
    {
        Guid SendTrackable(MailMessage mail, bool normalize = true);
        Task<Guid> SendTrackableAsync(MailMessage mail, bool normalize = true);
    }
}
