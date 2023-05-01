using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AboveGoal.Prospects.Import;
using Abp.Application.Features;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Timing;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.Imports;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.SecureFiles;
using DispatcherWeb.Web.Areas.App.Models.Imports;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Imports)]
    public class ImportsController : DispatcherWebControllerBase
    {
        private readonly ISecureFileBlobService _secureFileBlobService;
        private readonly ISecureFilesAppService _secureFilesService;

        public ImportsController(
            ISecureFileBlobService secureFileBlobService,
            ISecureFilesAppService secureFilesService
            )
        {
            _secureFileBlobService = secureFileBlobService;
            _secureFilesService = secureFilesService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_FuelUsage)]
        [Route("app/ImportFuel")]
        public IActionResult FuelUsage()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_VehicleUsage)]
        [Route("app/ImportVehicle")]
        public IActionResult VehicleUsage()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Customers)]
        [RequiresFeature(AppFeatures.QuickbooksImportFeature)]
        [Route("app/ImportCustomers")]
        public virtual IActionResult Customers()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Trucks)]
        [RequiresFeature(AppFeatures.QuickbooksImportFeature)]
        [Route("app/ImportTrucks")]
        public virtual IActionResult Trucks()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Vendors)]
        [RequiresFeature(AppFeatures.QuickbooksImportFeature)]
        [Route("app/ImportVendors")]
        public virtual IActionResult Vendors()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Services)]
        [RequiresFeature(AppFeatures.QuickbooksImportFeature)]
        [Route("app/ImportServices")]
        public virtual IActionResult Services()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Employees)]
        [RequiresFeature(AppFeatures.QuickbooksImportFeature)]
        [Route("app/ImportEmployees")]
        public virtual IActionResult Employees()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.AllowImportingTruxEarnings)]
        [RequiresFeature(AppFeatures.AllowImportingTruxEarnings)]
        [Route("app/TruxImport")]
        public virtual IActionResult Trux()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.AllowImportingLuckStoneEarnings)]
        [RequiresFeature(AppFeatures.AllowImportingLuckStoneEarnings)]
        [Route("app/LuckStoneImport")]
        public virtual IActionResult LuckStone()
        {
            return View();
        }

        public PartialViewResult CancelModal()
        {
            return PartialView("_CancelModal");
        }

        [Route("app/ImportResults/{id}/{fileName}")]
        public async Task<IActionResult> ImportResult(
            Guid id,
            string fileName
        )
        {
            string blobName = $"{id}/{fileName}";
            var metadata = await _secureFileBlobService.GetMetadataAsync(blobName);
            string validationResultJsonString = await _secureFileBlobService.GetChildBlobAsync(blobName, SecureFileChildFileNames.ImportResult);
            var validationResult = Utility.Deserialize<ImportResultDto>(validationResultJsonString);

            return View(validationResult);
        }

        [Modal]
        public PartialViewResult ImportMappingModal(
            Guid id,
            string fileName,
            ImportType importType
        )
        {
            var model = GetImportMappingViewModel(_secureFileBlobService, id, fileName, importType);

            return PartialView("_ImportMappingModal", model);
        }

        [Modal]
        public PartialViewResult ImportJacobusEnergyModal(
            Guid id,
            string fileName,
            ImportType importType
        )
        {
            var model = GetImportMappingViewModel(_secureFileBlobService, id, fileName, importType);

            return PartialView("_ImportJacobusEnergyModal", model);
        }

        [Modal]
        public PartialViewResult ImportWithNoMappingModal(
            Guid id,
            string fileName,
            ImportType importType
        )
        {
            var model = GetImportMappingViewModel(_secureFileBlobService, id, fileName, importType);

            return PartialView("_ImportWithNoMappingModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_VehicleUsage)]
        public IActionResult ImportVehicleModal()
        {
            ViewBag.ImportType = ImportType.VehicleUsage;
            return PartialView("_ImportVehicleModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_FuelUsage)]
        public IActionResult ImportFuelModal()
        {
            ViewBag.ImportType = ImportType.FuelUsage;
            return PartialView("_ImportFuelModal");
        }

        private static ImportMappingViewModel GetImportMappingViewModel(ISecureFileBlobService secureFileBlobService, Guid id, string fileName, ImportType importType)
        {
            ImportMappingViewModel model = new ImportMappingViewModel
            {
                BlobName = $"{id}/{fileName}",
                ImportType = importType
            };

            using (var fileStream = new MemoryStream(secureFileBlobService.GetSecureFile(id, fileName)))
            using (TextReader textReader = new StreamReader(fileStream))
            {
                IImportReader reader = new ImportReader(textReader);
                model.CsvFields = reader.GetCsvHeaders();
            }

            return model;
        }

        public IActionResult GetImportFields(ImportType importType)
        {
            var fields1 = StandardFields.GetFields(importType)
                .Where(f => f.Group == null)
                .Select(f => new
                {
                    id = f.Name.ToLowerInvariant(),
                    text = f.Name,
                    allowMulti = f.AllowMulti,
                    isRequired = f.IsRequired,
                    requireOnlyOneOf = f.RequireOnlyOneOf?.Select(s => s.ToLowerInvariant()).ToArray(),
                }).ToArray();

            var fields2 = StandardFields.GetFields(importType)
                .GroupBy(f => f.Group, f => new { f.Name, f.AllowMulti, f.IsRequired })
                .Where(x => x.Key != null)
                .Select(x => new
                {
                    text = x.Key,
                    children = x.Select(f => new { id = f.Name.ToLowerInvariant(), text = f.Name, allowMulti = f.AllowMulti, isRequired = f.IsRequired })
                }).ToArray();

            return new JsonResult(new { fields1, fields2 });
        }

        public async Task DeleteDonorFile(string blobName)
        {
            await _secureFileBlobService.DeleteSecureFileAsync(blobName);
        }


        public async Task<IActionResult> UploadFile()
        {
            var file = Request.Form.Files.Any() ? Request.Form.Files[0] : null;
            string importAddTypeString = Request.Form["importAddType"].FirstOrDefault();
            Enum.TryParse(importAddTypeString, out ImportType importType);

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
