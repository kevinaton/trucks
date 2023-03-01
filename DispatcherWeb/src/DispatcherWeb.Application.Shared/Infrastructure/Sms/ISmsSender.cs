using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Sms.Dto;

namespace DispatcherWeb.Infrastructure.Sms
{
    public interface ISmsSender
    {
        Task<SmsSendResult> SendAsync(string text, string toPhoneNumber, bool trackStatus = false, bool useTenantPhoneNumberOnly = false, bool insertEntity = true);
    }
}