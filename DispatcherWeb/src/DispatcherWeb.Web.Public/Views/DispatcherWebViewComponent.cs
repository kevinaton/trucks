using Abp.AspNetCore.Mvc.ViewComponents;

namespace DispatcherWeb.Web.Public.Views
{
    public abstract class DispatcherWebViewComponent : AbpViewComponent
    {
        protected DispatcherWebViewComponent()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }
    }
}