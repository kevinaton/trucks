using Abp.Authorization;
using System.Threading.Tasks;

namespace DispatcherWeb.Features
{
    [AbpAuthorize]
    public class FeaturesAppService : DispatcherWebAppServiceBase
    {
        public async Task<bool> IsFeatureEnabled(string featureName)
        {
            return await IsEnabledAsync(featureName);
        }
    }
}