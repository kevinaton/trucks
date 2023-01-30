using Abp.AspNetCore.Mvc.Views;

namespace DispatcherWeb.Web.Views
{
    public abstract class DispatcherWebRazorPage<TModel> : AbpRazorPage<TModel>
    {
        protected DispatcherWebRazorPage()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }
    }
}
