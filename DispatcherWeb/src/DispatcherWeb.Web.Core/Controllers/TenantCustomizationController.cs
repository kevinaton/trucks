using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetZeroCore.Net;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Storage;
using DispatcherWeb.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    [AbpMvcAuthorize]
    public class TenantCustomizationController : DispatcherWebControllerBase
    {
        private readonly TenantManager _tenantManager;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public TenantCustomizationController(
            TenantManager tenantManager,
            IBinaryObjectManager binaryObjectManager)
        {
            _tenantManager = tenantManager;
            _binaryObjectManager = binaryObjectManager;
        }

        public enum LogoType
        {
            ApplicationLogo,
            ReportsLogo,
        }

        [HttpPost]
        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Tenant_Settings)]
        public async Task<JsonResult> UploadLogo(LogoType logoType)
        {
            try
            {
                var logoFile = Request.Form.Files.First();

                //Check input
                if (logoFile == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                if (logoFile.Length > LogoMaxSize())
                {
                    throw new UserFriendlyException(L("File_SizeLimit_Error"));
                }

                byte[] fileBytes;
                using (var stream = logoFile.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var imageFormat = ImageFormatHelper.GetRawImageFormat(fileBytes);
                if (!imageFormat.IsIn(ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif))
                {
                    throw new UserFriendlyException(L("File_Invalid_Type_Error"));
                }

                var logoObject = new BinaryObject(AbpSession.GetTenantId(), fileBytes, $"Logo {DateTime.UtcNow}");
                await _binaryObjectManager.SaveAsync(logoObject);

                var tenant = await _tenantManager.GetByIdAsync(AbpSession.GetTenantId());
                switch (logoType)
                {
                    case LogoType.ApplicationLogo:
                        tenant.LogoId = logoObject.Id;
                        tenant.LogoFileType = logoFile.ContentType;
                        break;
                    case LogoType.ReportsLogo:
                        tenant.ReportsLogoId = logoObject.Id;
                        tenant.ReportsLogoFileType = logoFile.ContentType;
                        break;
                }

                return Json(new AjaxResponse(new { id = logoObject.Id, TenantId = tenant.Id, fileType = tenant.LogoFileType }));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }

            // Local functions
            int LogoMaxSize()
            {
                switch (logoType)
                {
                    case LogoType.ApplicationLogo:
                        return 30270; // 30KB
                    case LogoType.ReportsLogo:
                        return 300 * 1014; // 300KB
                    default:
                        throw new ApplicationException("Not supported LogoType!");
                }
            }
        }

        [HttpPost]
        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Tenant_Settings)]
        public async Task<JsonResult> UploadCustomCss()
        {
            try
            {
                var cssFile = Request.Form.Files.First();

                //Check input
                if (cssFile == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                if (cssFile.Length > 1048576) //1MB
                {
                    throw new UserFriendlyException(L("File_SizeLimit_Error"));
                }

                byte[] fileBytes;
                using (var stream = cssFile.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var cssFileObject = new BinaryObject(AbpSession.GetTenantId(), fileBytes, $"Custom Css {cssFile.FileName} {DateTime.UtcNow}");
                await _binaryObjectManager.SaveAsync(cssFileObject);

                var tenant = await _tenantManager.GetByIdAsync(AbpSession.GetTenantId());
                tenant.CustomCssId = cssFileObject.Id;

                return Json(new AjaxResponse(new { id = cssFileObject.Id, TenantId = tenant.Id }));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetLogo(int? tenantId)
        {
            var defaultLogo = "/Common/Images/app-logo-dump-truck-130x35.gif";

            if (tenantId == null)
            {
                tenantId = AbpSession.TenantId;
            }
            if (!tenantId.HasValue)
            {
                //return StatusCode((int)HttpStatusCode.NotFound);
                return File(defaultLogo, MimeTypeNames.ImagePng);
            }

            var tenant = await _tenantManager.FindByIdAsync(tenantId.Value);
            if (tenant == null || !tenant.HasLogo())
            {
                //return StatusCode((int)HttpStatusCode.NotFound);
                return File(defaultLogo, MimeTypeNames.ImagePng);
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId.Value))
            {
                var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.LogoId.Value);
                if (logoObject == null)
                {
                    //return StatusCode((int)HttpStatusCode.NotFound);
                    return File(defaultLogo, MimeTypeNames.ImagePng);
                }

                return File(logoObject.Bytes, tenant.LogoFileType);
            }
        }

        [AllowAnonymous]
        [Route("/TenantCustomization/GetTenantLogo/{skin}/{tenantId?}")]
        [HttpGet]
        public Task<ActionResult> GetTenantLogoWithCustomRoute(string skin, int? tenantId = null)
        {
            return GetTenantLogo(skin, tenantId);
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetTenantLogo(string skin, int? tenantId)
        {
            var defaultLogo = "/Common/Images/app-logo-on-" + skin + ".svg";

            if (tenantId == null)
            {
                return File(defaultLogo, MimeTypeNames.ImagePng);
            }

            var tenant = await _tenantManager.FindByIdAsync(tenantId.Value);
            if (tenant == null || !tenant.HasLogo())
            {
                return File(defaultLogo, MimeTypeNames.ImagePng);
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId.Value))
            {
                var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.LogoId.Value);
                if (logoObject == null)
                {
                    return File(defaultLogo, MimeTypeNames.ImagePng);
                }

                return File(logoObject.Bytes, tenant.LogoFileType);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetCustomCss(int? tenantId)
        {
            if (tenantId == null)
            {
                tenantId = AbpSession.TenantId;
            }
            if (!tenantId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var tenant = await _tenantManager.FindByIdAsync(tenantId.Value);
            if (tenant == null || !tenant.CustomCssId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId.Value))
            {
                var cssFileObject = await _binaryObjectManager.GetOrNullAsync(tenant.CustomCssId.Value);
                if (cssFileObject == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                return File(cssFileObject.Bytes, MimeTypeNames.TextCss);
            }
        }
    }
}
