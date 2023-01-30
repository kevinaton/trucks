using System.Threading.Tasks;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.WebHooks
{
    public interface IAppWebhookPublisher
    {
        Task PublishTestWebhook();
    }
}
