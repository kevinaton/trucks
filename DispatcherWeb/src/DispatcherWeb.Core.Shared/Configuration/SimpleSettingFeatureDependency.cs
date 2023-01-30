using Abp.Application.Features;
using Abp.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Configuration
{
    public class SimpleSettingFeatureDependency : IFeatureDependency
    {
        public string[] Settings { get; set; }
        public bool RequiresAll { get; set; }

        public SimpleSettingFeatureDependency(bool requiresAll, params string[] settings)
            : this(settings)
        {
            RequiresAll = requiresAll;
        }

        public SimpleSettingFeatureDependency(params string[] settings)
        {
            Settings = settings;
        }

        public async Task<bool> IsSatisfiedAsync(IFeatureDependencyContext context)
        {
            var settingManager = context.IocResolver.Resolve<ISettingManager>();
            foreach (var setting in Settings)
            {
                var settingValue = await settingManager.GetSettingValueAsync<bool>(setting);
                if (settingValue)
                {
                    if (!RequiresAll)
                    {
                        return true;
                    }
                }
                else
                {
                    if (RequiresAll)
                    {
                        return false;
                    }
                }
            }
            return RequiresAll;
        }

        public bool IsSatisfied(IFeatureDependencyContext context)
        {
            var settingManager = context.IocResolver.Resolve<ISettingManager>();
            foreach (var setting in Settings)
            {
                var settingValue = settingManager.GetSettingValue<bool>(setting);
                if (settingValue)
                {
                    if (!RequiresAll)
                    {
                        return true;
                    }
                }
                else
                {
                    if (RequiresAll)
                    {
                        return false;
                    }
                }
            }
            return RequiresAll;
        }
    }
}
