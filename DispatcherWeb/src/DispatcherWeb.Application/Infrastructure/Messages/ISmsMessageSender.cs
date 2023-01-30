using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Messages
{
    public interface ISmsMessageSender
    {
        Task<bool> SendSmsMessageAndNotifyErrors(string cellPhoneNumber, string messageBody, string recipientName);
    }
}