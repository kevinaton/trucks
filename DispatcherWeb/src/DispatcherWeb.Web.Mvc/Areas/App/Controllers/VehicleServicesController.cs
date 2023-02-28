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
using DispatcherWeb.VehicleServices;
using DispatcherWeb.VehicleServices.Dto;
using DispatcherWeb.VehicleServiceTypes;
using DispatcherWeb.Web.Areas.App.Models.VehicleServices;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_VehicleService_View)]
    public class VehicleServicesController : DispatcherWebControllerBase
    {
        private readonly IVehicleServiceAppService _vehicleServiceAppService;
        private readonly IVehicleServiceTypeAppService _vehicleServiceTypeAppService;

        public VehicleServicesController(
            IVehicleServiceAppService vehicleServiceAppService,
            IVehicleServiceTypeAppService vehicleServiceTypeAppService
        )
        {
            _vehicleServiceAppService = vehicleServiceAppService;
            _vehicleServiceTypeAppService = vehicleServiceTypeAppService;
        }

        public IActionResult Index()
        {
            VehicleServiceListViewModel viewModel = new VehicleServiceListViewModel();
            return View(viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_VehicleService_Edit)]
        public async Task<PartialViewResult> CreateOrEditVehicleServiceModal(int? id)
        {
            var dto = await _vehicleServiceAppService.GetForEdit(new NullableIdDto(id));
            var viewModel = CreateOrEditVehicleServiceModalViewModel.CreateFromDto(dto);

            return PartialView("_CreateOrEditVehicleServiceModal", viewModel);
        }

        public async Task<IActionResult> UploadDocument()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.PaidFunctionality))
            {
                throw new ApplicationException(L("UpgradeToAccessThisFunctionality"));
            }

            var file = Request.Form.Files.Any() ? Request.Form.Files[0] : null;
            int vehicleServiceId = Int32.Parse(Request.Form["id"].First());

            if (file != null)
            {
                byte[] fileBytes;
                using (var fileStream = file.OpenReadStream())
                {
                    fileBytes = fileStream.GetAllBytes();
                }
                Guid fileId = AttachmentHelper.UploadToAzureBlob(fileBytes, vehicleServiceId, file.ContentType, AppConsts.VehicleServiceDocumentsContainerName);
                var document = await _vehicleServiceAppService.SaveDocument(new VehicleServiceDocumentEditDto()
                {
                    FileId = fileId,
                    VehicleServiceId = vehicleServiceId,
                    Name = file.FileName.Split('.').FirstOrDefault(),
                });

                return Json(new AjaxResponse(new
                {
                    id = document.Id,
                    vehicleServiceId = document.VehicleServiceId,
                    fileId = document.FileId,
                    name = document.Name,
                    description = document.Description,
                }));
            }

            return Json(new AjaxResponse(false));

        }

        public IActionResult DownloadDocument(int vehicleServiceId, Guid fileId, string fileName)
        {
            byte[] fileContent = AttachmentHelper.GetFromAzureBlob($"{vehicleServiceId}/{fileId}", AppConsts.VehicleServiceDocumentsContainerName).Content;
            return File(fileContent, "application/octet-stream", fileName + ".pdf");
        }

    }
}
