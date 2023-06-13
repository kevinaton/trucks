using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Export.Pdf.Page;
using GrapeCity.ActiveReports.Rendering.IO;
using GrapeCity.ActiveReports.Web.Viewer;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HttpClient = System.Net.Http.HttpClient;
using PdfSettings = GrapeCity.ActiveReports.Export.Pdf.Page.Settings;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions
{
    public class VehicleMaintenanceWorkOrderReportDataDefinitions : ReportDataDefinitionBase
    {
        private readonly ReportAppService _reportAppService;
        private readonly IHostEnvironment _environment;

        public VehicleMaintenanceWorkOrderReportDataDefinitions(IHostEnvironment environment,
                                        ReportAppService reportAppService,
                                        IConfiguration configuration,
                                        IHttpContextAccessor httpContextAccessor,
                                        ILoggerFactory loggerFactory)

                    : base(configuration, httpContextAccessor, loggerFactory)
        {
            _reportAppService = reportAppService;
            _environment = environment;
        }

        public override bool HasTenantsParameter => false;

        public override async Task Initialize()
        {
            var getReportResult = await _reportAppService.TryGetReport(ReportId);

            if (!getReportResult.Success)
                throw new Exception("Report is not registered.");

            if (!getReportResult.ReportInfo.HasAccess)
                throw new Exception("You do not have access to view this report.");

            var reportsDirPath = new DirectoryInfo($"{_environment.ContentRootPath}\\Reports\\");
            var reportPath = $"{Path.Combine(reportsDirPath.FullName, getReportResult.ReportInfo.Path)}.rdlx";

            ThisPageReport = new PageReport(new FileInfo(reportPath));
            ThisPageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderDataSource");
            ThisPageReport.Document.PageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderDataSource");
            ThisPageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderLinesDataSource");
            ThisPageReport.Document.PageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderLinesDataSource");

            await base.Initialize();
        }

        public override MemoryStream OpenReportAsPdf(int? entityId)
        {
            base.OpenReportAsPdf(entityId);

            // Provide settings for the rendering output.
            var pdfSetting = new PdfSettings();

            //Set the rendering extension and render the report.
            var pdfRenderingExtension = new PdfRenderingExtension();
            var outputProvider = new MemoryStreamProvider();
            ThisPageReport.Document.Render(pdfRenderingExtension, outputProvider, pdfSetting);

            var memStream = (MemoryStream)outputProvider.GetPrimaryStream().OpenStream();
            return memStream;
        }

        public override async Task<(bool IsMasterDataSource, object DataSourceJson)> LocateDataSource(LocateDataSourceArgs arg)
        {
            var contentJson = _emptyArrayInResult;
            var (isMasterDataSource, dataSourceJson) = await base.LocateDataSource(arg);
            if (isMasterDataSource)
            {
                return (isMasterDataSource, dataSourceJson);
            }

            if (arg.DataSet.Name.Equals("VehicleMaintenanceWorkOrderDataSet"))
            {
                contentJson = await GetVehicleMaintenanceWorkOrderDataSource(arg);
            }
            else if (arg.DataSet.Name.Equals("VehicleMaintenanceWorkOrderLinesDataSet"))
            {
                contentJson = await GetVehicleMaintenanceWorkOrderLinesDataSource(arg);
            }
            return (isMasterDataSource, contentJson);
        }

        #region private members

        internal async Task<string> GetVehicleMaintenanceWorkOrderLinesDataSource(LocateDataSourceArgs arg)
        {
            var paramsDic = arg.Parameters.ToDictionary(p => p.Name, p => p.Value);
            var reportParamsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);
            int? entityId = null;
            if (reportParamsDic["EntityId"] != null)
                entityId = int.Parse(reportParamsDic["EntityId"].ToString());

            var result = await GetVehicleMaintenanceWorkOrderLinesJson(entityId);
            return result;
        }

        internal async Task<string> GetVehicleMaintenanceWorkOrderLinesJson(int? entityId)
        {
            var hostApiUrl = Configuration["IdentityServer:Authority"];
            var accessToken = await base.HttpContextAccessor.HttpContext.GetTokenAsync("access_token");

            using var client = new HttpClient();
            client.SetBearerToken(accessToken);

            var url = $"{hostApiUrl}/api/services/app/workorder/GetWorkOrderLines?id={entityId}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                Logger.Log(LogLevel.Error, $"Error: {Extensions.GetMethodName()} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
            }
            else
            {
                var contentJson = await response.Content.ReadAsStringAsync();
                Logger.Log(LogLevel.Information, $"Success: {Extensions.GetMethodName()} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
                return contentJson;
            }

            return "[]";
        }

        internal async Task<string> GetVehicleMaintenanceWorkOrderDataSource(LocateDataSourceArgs arg)
        {            
            var hostApiUrl = Configuration["IdentityServer:Authority"];
            var paramsDic = arg.Parameters.ToDictionary(p => p.Name, p => p.Value);
            var reportParamsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);
            var accessToken = await base.HttpContextAccessor.HttpContext.GetTokenAsync("access_token");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{hostApiUrl}/api/services/app/workorder/getworkorderforedit?id={reportParamsDic["EntityId"]}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                Logger.Log(LogLevel.Error, $"Error: {Extensions.GetMethodName()} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
            }
            else
            {
                var contentJson = await response.Content.ReadAsStringAsync();
                Logger.Log(LogLevel.Information, $"Success: {Extensions.GetMethodName()} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
                return contentJson;
            }

            return "{}";
        }

        #endregion
    }
}
