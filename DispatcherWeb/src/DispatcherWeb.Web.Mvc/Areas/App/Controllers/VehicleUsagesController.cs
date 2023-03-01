using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AboveGoal.Prospects.Import;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Timing;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.SecureFiles;
using DispatcherWeb.VehicleUsages;
using DispatcherWeb.Web.Areas.App.Models.VehicleUsages;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_VehicleUsages_View)]
    public class VehicleUsagesController : DispatcherWebControllerBase
    {
        private readonly IVehicleUsageAppService _vehicleUsageAppService;
        private readonly ISecureFilesAppService _secureFilesService;
        private readonly ISecureFileBlobService _secureFileBlobService;

        public VehicleUsagesController(
            IVehicleUsageAppService vehicleUsageAppService,
            ISecureFilesAppService secureFilesService,
            ISecureFileBlobService secureFileBlobService
        )
        {
            _vehicleUsageAppService = vehicleUsageAppService;
            _secureFilesService = secureFilesService;
            _secureFileBlobService = secureFileBlobService;
        }

        [Route("app/vehicleusages")]
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_VehicleUsages_Edit)]
        public async Task<PartialViewResult> CreateOrEditVehicleUsageModal(int? id, int? officeId)
        {
            var dto = await _vehicleUsageAppService.GetVehicleUsageForEdit(new NullableIdDto(id));
            var model = CreateOrEditVehicleUsageModalViewModel.CreateFromVehicleUsageEditDto(dto);
            model.OfficeId = officeId;
            return PartialView("_CreateOrEditVehicleUsageModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_VehicleUsage)]
        public IActionResult ImportVehicleUsageModal()
        {
            ViewBag.ImportType = ImportType.VehicleUsage;
            return PartialView("_ImportVehicleUsageModal");
        }

        public async Task<IActionResult> UploadFile()
        {
            var file = Request.Form.Files.Any() ? Request.Form.Files[0] : null;

            ImportType importType = ImportType.VehicleUsage;
            if (file != null)
            {
                Guid id = await _secureFilesService.GetSecureFileDefinitionId();
                string fileName = $"Import_{Clock.Now:yyyyMMddHHmmss}.csv";
                using (var fileStream = file.OpenReadStream())
                {
                    if (fileStream.Length == 0)
                    {
                        return Json(new AjaxResponse(false));
                    }
                    _secureFileBlobService.UploadSecureFile(
                        fileStream,
                        id,
                        fileName,
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>(ImportBlobMetadata.UtcDateTime, DateTime.UtcNow.ToString("O")),
                        });

                }

                return Json(new AjaxResponse(new { id = id, blobName = fileName, importType = importType }));
            }

            return Json(new AjaxResponse(false));
        }
    }
}
