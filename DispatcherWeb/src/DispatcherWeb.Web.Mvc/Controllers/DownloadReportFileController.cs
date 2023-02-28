using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Net.MimeTypes;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    public class DownloadReportFileController : AbpController
    {
        [AbpMvcAuthorize]
        [DisableAuditing]
        public ActionResult Index(FileDto file)
        {
            byte[] fileBytes = AttachmentHelper.GetReportFile(AbpSession.UserId.ToString(), file.FileToken);
            if (fileBytes.Length == 0)
            {
                return NotFound();
            }
            if (file.FileType == MimeTypeNames.ApplicationPdf)
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=" + file.FileName);
                return File(fileBytes, file.FileType);
            }
            return File(fileBytes, file.FileType, file.FileName);
        }

    }
}
