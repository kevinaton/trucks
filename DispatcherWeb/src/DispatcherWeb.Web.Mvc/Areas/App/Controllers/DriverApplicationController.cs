using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.Storage;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_DriverApplication)]
    public class DriverApplicationController : DispatcherWebControllerBase
    {
        private readonly IDriverApplicationAppService _driverApplicationAppService;
        private readonly IDispatchingAppService _dispatchingAppService;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IConfigurationRoot _configuration;

        public DriverApplicationController(
            IDriverApplicationAppService driverApplicationAppService,
            IDispatchingAppService dispatchingAppService,
            IBinaryObjectManager binaryObjectManager,
            IWebHostEnvironment env
        )
        {
            _driverApplicationAppService = driverApplicationAppService;
            _dispatchingAppService = dispatchingAppService;
            _binaryObjectManager = binaryObjectManager;
            _configuration = env.GetAppConfiguration();
        }

        public ActionResult Index()
        {
            //todo: use per-tenant setting instead, with a fallback to an application wide setting.
            if (_configuration["App:DriverApplicationVersion"] == "2")
            {
                return Redirect(_configuration["App:DriverApplicationUri"]);
            }
            else //if (_configuration["App:DriverApplicationVersion"] == "3")
            {
                //todo add a custom view that will try to redirect to com.dumptruckdispatcher.driver://auth, but if it fails, then redirect to google play store page (or other store)
                return Redirect(_configuration["App:DriverApplication3Uri"]);
            }

            //if (!await _driverApplicationAppService.UserIsClockedIn())
            //{
            //    var scheduledStartTime = await _driverApplicationAppService.GetScheduledStartTime();
            //    return View("ClockIn", scheduledStartTime);
            //}

            //var driverInfoDto = await _dispatchingAppService.GetDriverInfoForCurrentUser();
            //switch (driverInfoDto)
            //{
            //    case DriverInfoNotFoundDto _:
            //        var noDispatchViewModel = await GetDriverApplicationClockViewModel(new DriverInfoBaseViewModel());
            //        return View("NoDispatch", noDispatchViewModel);
            //    case DriverLoadInfoDto driverLoadInfoDto when driverLoadInfoDto.DispatchStatus == DispatchStatus.Sent || driverLoadInfoDto.DispatchStatus == DispatchStatus.Created:
            //        var acknowledgeModel = await GetDriverApplicationClockViewModel(DriverLoadInfoViewModel.CreateFrom(driverLoadInfoDto));
            //        return View("AcknowledgeDispatch", acknowledgeModel);
            //    case DriverLoadInfoDto driverLoadInfoDto when driverLoadInfoDto.DispatchStatus == DispatchStatus.Acknowledged:
            //        var loadModel = await GetDriverApplicationClockViewModel(DriverLoadInfoViewModel.CreateFrom(driverLoadInfoDto));
            //        return View("LoadInfo", loadModel);
            //    case DriverDestinationInfoDto driverDestinationInfoDto:
            //        var destinationModel = await GetDriverApplicationClockViewModel(DriverDestinationInfoViewModel.CreateFrom(driverDestinationInfoDto));
            //        return View("CompleteDelivery", destinationModel);
            //    default:
            //        throw new ApplicationException("Unexpected");
            //}
        }

        //private async Task<DriverApplicationDriverInfoViewModel<T>> GetDriverApplicationClockViewModel<T>(T driverInfoViewModel) where T : new()
        //{
        //    var elapsedTimeResult = await _driverApplicationAppService.GetElapsedTime();
        //    return new DriverApplicationDriverInfoViewModel<T>
        //    {
        //        ElapsedTime = elapsedTimeResult.ElapsedTime,
        //        ClockIsStarted = elapsedTimeResult.ClockIsStarted,
        //        DriverInfoViewModel = driverInfoViewModel,
        //    };
        //}

        [Modal]
        public PartialViewResult AddSignatureModal(AddSignatureInput input)
        {
            return PartialView("_AddSignatureModal", input);
        }

        [Modal]
        [AllowAnonymous]
        public PartialViewResult AddTicketPhotoModal(AddTicketPhotoInput input)
        {
            return PartialView("_AddTicketPhotoModal", input);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> AddTicketPhoto(AddTicketPhotoInput input)
        {
            try
            {
                var file = Request.Form.Files.First();

                //Check input
                if (file == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                if (file.Length > 8388608) //8MB
                {
                    throw new UserFriendlyException(L("File_SizeLimit_Error"));
                }

                byte[] fileBytes;
                using (var stream = file.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var tenantId = await _dispatchingAppService.GetTenantIdFromDispatch(input.Guid);

                var fileObject = new BinaryObject(tenantId, fileBytes);
                await _binaryObjectManager.SaveAsync(fileObject);

                return Json(new AjaxResponse(new { id = fileObject.Id }));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }
    }
}
