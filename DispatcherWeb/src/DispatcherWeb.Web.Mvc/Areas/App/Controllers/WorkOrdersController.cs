using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.WorkOrders;
using DispatcherWeb.WorkOrders.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_WorkOrders_View)]
    public class WorkOrdersController : DispatcherWebControllerBase
    {
        private readonly IWorkOrderAppService _workOrderAppService;

        public WorkOrdersController(
            IWorkOrderAppService workOrderAppService
        )
        {
            _workOrderAppService = workOrderAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? id)
        {
            WorkOrderEditDto model = await _workOrderAppService.GetWorkOrderForEdit(new NullableIdDto(id));
            return View(model);
        }

        public async Task<PartialViewResult> CreateOrEditWorkOrderLineModal(int? id, int workOrderId)
        {
            var model = await _workOrderAppService.GetWorkOrderLineForEdit(new GetWorkOrderLineForEditInput(id, workOrderId));
            return PartialView("_CreateOrEditWorkOrderLineModal", model);
        }

        public async Task<IActionResult> UploadPicture()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.PaidFunctionality))
            {
                throw new ApplicationException(L("UpgradeToAccessThisFunctionality"));
            }

            var file = Request.Form.Files.Any() ? Request.Form.Files[0] : null;
            int workOrderId = Int32.Parse(Request.Form["id"].First());

            if (file != null)
            {
                byte[] fileBytes;
                using (var fileStream = file.OpenReadStream())
                {
                    fileBytes = fileStream.GetAllBytes();
                }
                Guid fileId = AttachmentHelper.UploadToAzureBlob(fileBytes, workOrderId, file.ContentType, AppConsts.WorkOrderPicturesContainerName);
                var document = await _workOrderAppService.SavePicture(new WorkOrderPictureEditDto()
                {
                    FileId = fileId,
                    WorkOrderId = workOrderId,
                    FileName = file.FileName,
                });

                return Json(new AjaxResponse(new
                {
                    id = document.Id,
                    workOrderId = document.WorkOrderId,
                    fileId = document.FileId,
                    fileName = document.FileName,
                }));
            }

            return Json(new AjaxResponse(false));

        }

        public async Task<PartialViewResult> GetPictureRow(int id)
        {
            var model = await _workOrderAppService.GetPictureEditDto(id);
            return PartialView("_Picture", model);
        }

        public IActionResult DownloadPicture(int workOrderId, Guid fileId, string fileName)
        {
            byte[] fileContent = AttachmentHelper.GetFromAzureBlob($"{workOrderId}/{fileId}", AppConsts.WorkOrderPicturesContainerName).Content;
            return File(fileContent, "application/octet-stream", fileName);
        }


    }
}
