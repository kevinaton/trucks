using Abp.Dependency;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.Test.Base.Configuration
{
    public class TestAppConfigurationAccessor : IAppConfigurationAccessor, ISingletonDependency
    {
        public IConfigurationRoot Configuration { get; }

        public TestAppConfigurationAccessor()
        {
            Configuration = AppConfigurations.Get(
                typeof(DispatcherWebTestBaseModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }
    }
}
