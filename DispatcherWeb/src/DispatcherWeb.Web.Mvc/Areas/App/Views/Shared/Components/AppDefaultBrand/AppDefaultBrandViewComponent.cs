using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Web.Areas.App.Models.Layout;
using DispatcherWeb.Web.Session;
using DispatcherWeb.Web.Views;

namespace DispatcherWeb.Web.Areas.App.Views.Shared.Components.AppDefaultBrand
{
    public class AppDefaultBrandViewComponent : DispatcherWebViewComponent
    {
        private readonly IPerRequestSessionCache _sessionCache;

        public AppDefaultBrandViewComponent(IPerRequestSessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var headerModel = new HeaderViewModel
            {
                LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync()
            };

            return View(headerModel);
        }
    }
}
