using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Timing.Timezone;
using CsvHelper;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports.Dto;
using DispatcherWeb.Net.MimeTypes;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace DispatcherWeb.Infrastructure.Reports
{
    public abstract class ReportAppServiceBase<TInput> /*: ITransientDependency*/ : DispatcherWebAppServiceBase where TInput : class
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private CustomSession _customSession;

        protected ReportAppServiceBase(
            ITimeZoneConverter timeZoneConverter
        )
        {
            _timeZoneConverter = timeZoneConverter;
        }

        public CustomSession CustomSession
        {
            get
            {
                if (_customSession == null)
                {
                    if (AbpSession.TenantId == null || AbpSession.UserId == null)
                    {
                        throw new ApplicationException("CustomSession must be set when running in a separate process!");
                    }
                    _customSession = new CustomSession(AbpSession.TenantId.Value, AbpSession.UserId.Value);
                }
                return _customSession;
            }
            set
            {
                _customSession = value;
            }
        }

        public async Task<FileDto> CreatePdf(TInput input)
        {
            await CheckPermissions();
            var file = new FileDto(await GetReportFilename("pdf", input), MimeTypeNames.ApplicationPdf);

            Document document = new Document();
            document.DefineStyles();
            PdfReport report = new PdfReport(document, GetLocalDateTimeNow().ToString("g"));
            InitPdfReport(report);

            if (await CreatePdfReport(report, input))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(false) { Document = document };
                    pdfRenderer.RenderDocument();
                    pdfRenderer.PdfDocument.Save(stream, false);
                    AttachmentHelper.UploadReportFile(stream, CustomSession.UserId.ToString(), file.FileToken);
                }
            }
            else
            {
                file.FileName = "";
            }

            return file;
        }

        public async Task<FileDto> CreateCsv(TInput input)
        {
            await CheckPermissions();
            var file = new FileDto(await GetReportFilename("csv", input), MimeTypeNames.TextCsv);
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                CsvReport report = new CsvReport(csv);
                if (await CreateCsvReport(report, input))
                {
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    AttachmentHelper.UploadReportFile(stream, CustomSession.UserId.ToString(), file.FileToken);
                }
                else
                {
                    file.FileName = "";
                }

            }
            return file;
        }

        private async Task CheckPermissions()
        {
            if (!await PermissionChecker.IsGrantedAsync(AbpSession.ToUserIdentifier(), ReportPermission))
            {
                throw new AbpAuthorizationException($"The user don't have the {ReportPermission} permission for this report.");
            }
        }
        protected abstract string ReportPermission { get; }

        protected DateTime GetLocalDateTimeNow()
        {
            return _timeZoneConverter.Convert(Clock.Now, CustomSession.TenantId, CustomSession.UserId)
                ?? Clock.Now;
        }

        protected virtual Task<string> GetReportFilename(string extension, TInput input)
        {
            return Task.FromResult($"{ReportFileName}_{GetLocalDateTimeNow():yyyy_MM_dd}.{extension}".SanitizeFilename());
        }

        protected abstract string ReportFileName { get; }

        protected abstract void InitPdfReport(PdfReport report);
        protected abstract Task<bool> CreatePdfReport(PdfReport report, TInput input);
        protected abstract Task<bool> CreateCsvReport(CsvReport report, TInput input);

    }
}
