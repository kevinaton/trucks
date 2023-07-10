using Abp.Application.Services;
using System.Threading.Tasks;

namespace DispatcherWeb.Features
{
    public interface IFeaturesAppService : IApplicationService
    {
        Task<bool> IsFeatureEnabled(string featureName);
    }
}