using System.Threading.Tasks;

namespace DispatcherWeb.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}