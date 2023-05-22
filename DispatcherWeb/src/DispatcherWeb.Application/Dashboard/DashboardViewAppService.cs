using Abp.Application.Services;
using DispatcherWeb.Services;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard
{
    public class DashboardViewAppService : ApplicationService
    {
        private readonly IViewRenderService _viewRenderService;
        private readonly IDashboardAppService _dashboardService;

        public DashboardViewAppService(
            IViewRenderService viewRenderService,
            IDashboardAppService dashboardService)
        {
            _viewRenderService = viewRenderService;
            _dashboardService = dashboardService;
        }

        public async Task<string> GetScheduledTruckCountPartialView()
        {
            var model = await _dashboardService.GetScheduledTruckCountDto();
            return await _viewRenderService.RenderToStringAsync("~/Areas/App/Views/Dashboard/_ScheduledTruckCount.cshtml", model);
        }
    }
}