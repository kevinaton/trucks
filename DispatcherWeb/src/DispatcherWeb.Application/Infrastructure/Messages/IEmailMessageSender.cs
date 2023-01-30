using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Messages
{
    public interface IEmailMessageSender
    {
        Task<bool> SendEmailMessageAndNotifyErrors(string fromEmailAddress, string toEmailAddress, string subject, string messageBody, string recipientName);
    }
}