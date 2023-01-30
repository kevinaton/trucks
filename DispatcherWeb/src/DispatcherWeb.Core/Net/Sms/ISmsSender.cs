using System.Threading.Tasks;

namespace DispatcherWeb.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}