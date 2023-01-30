using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using DispatcherWeb.Notifications;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using DispatcherWeb.Notifications.Dto;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class NotificationsController : DispatcherWebControllerBase
    {
        private readonly INotificationAppService _notificationApp;

        public NotificationsController(INotificationAppService notificationApp)
        {
            _notificationApp = notificationApp;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> SettingsModal()
        {
            var notificationSettings = await _notificationApp.GetNotificationSettings();
            return PartialView("_SettingsModal", notificationSettings);
        }

        [Modal]
        public PartialViewResult PriorityNotificationModal(PriorityNotificationDto model)
        {
            return PartialView("_PriorityNotificationModal", model);
        }
    }
}