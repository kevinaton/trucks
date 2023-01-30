using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.UnitsOfMeasure;
using DispatcherWeb.Web.Areas.App.Models.Acknowledge;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    public class AcknowledgeController : DispatcherWebControllerBase
    {
        private readonly IDispatchingAppService _dispatchingAppService;

        public AcknowledgeController(
            IDispatchingAppService dispatchingAppService
        )
        {
            _dispatchingAppService = dispatchingAppService;
        }

        [AllowAnonymous]
        [Route("app/acknowledge/{shortGuid}")]
        public async Task<ActionResult> Index(string shortGuid, bool editTicket)
        {
            ShortGuid.TryParse(shortGuid, out var acknowledgeGuid);
            var driverInfoDto = await _dispatchingAppService.GetDriverInfo(new GetDriverInfoInput()
            {
                AcknowledgeGuid = acknowledgeGuid,
                EditTicket = editTicket,
            });

            switch (driverInfoDto)
            {
                case DriverInfoNotFoundDto _:
                    return View("../../../../Views/Error/Error404");
                case DriverInfoDeletedDto _:
                    return await GetIndexViewThereIsNextDispatchTodayOrDefaultView("Deleted");
                case DriverLoadInfoDto driverLoadInfoDto when driverLoadInfoDto.DispatchStatus == DispatchStatus.Created || driverLoadInfoDto.DispatchStatus == DispatchStatus.Sent:
                    DriverLoadInfoViewModel acknowledgeModel = DriverLoadInfoViewModel.CreateFrom(driverLoadInfoDto);
                    acknowledgeModel.Guid = acknowledgeGuid.Guid;
                    return View("DriverAcknowledge", acknowledgeModel);
                case DriverLoadInfoDto driverLoadInfoDto:
                    DriverLoadInfoViewModel loadModel = DriverLoadInfoViewModel.CreateFrom(driverLoadInfoDto);
                    loadModel.Guid = acknowledgeGuid.Guid;
                    return View("DriverLoadInfo", loadModel);
                case DriverDestinationInfoDto driverDestinationInfoDto:
                    DriverDestinationInfoViewModel destinationModel = DriverDestinationInfoViewModel.CreateFrom(driverDestinationInfoDto);
                    destinationModel.Guid = acknowledgeGuid.Guid;
                    return View("DriverDestinationInfo", destinationModel);
                case DriverInfoCompletedDto _:
                    return await GetIndexViewThereIsNextDispatchTodayOrDefaultView("Completed");
                case DriverInfoExpiredDto _:
                    return View("Expired");
                case DriverInfoCanceledDto _:
                    return await GetIndexViewThereIsNextDispatchTodayOrDefaultView("Canceled");
                case DriverInfoErrorAndRedirect redirectInfoDto:
                    return View("ErrorWithRedirect", redirectInfoDto);
                default:
                    throw new ApplicationException("Unexpected");
            }

            // Local functions
            async Task<ActionResult> GetIndexViewThereIsNextDispatchTodayOrDefaultView(string defaultViewName)
            {
                var nextDispatch = await _dispatchingAppService.GetNextDispatchToday(acknowledgeGuid);
                if (nextDispatch.DispatchExists)
                {
                    return RedirectToAction("Index", new { shortGuid = nextDispatch.DispatchGuid.ToShortGuid() });
                }
                return View(defaultViewName);
            }
        }

        [AllowAnonymous]
        [Route("app/acknowledge/completed")]
        public ActionResult Completed()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("app/acknowledge/next")]
        public ActionResult Next()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("app/acknowledge/expired")]
        public ActionResult Expired()
        {
            return View();
        }

    }
}
