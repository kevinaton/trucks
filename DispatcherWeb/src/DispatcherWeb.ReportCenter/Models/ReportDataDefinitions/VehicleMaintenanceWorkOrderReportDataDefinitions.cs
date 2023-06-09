using System;
using System.IO;
using System.Linq;
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
using HttpClient = System.Net.Http.HttpClient;
using Settings = GrapeCity.ActiveReports.Export.Pdf.Page.Settings;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions
{
    public class VehicleMaintenanceWorkOrderReportDataDefinitions : ReportDataDefinitionBase
    {
        private readonly ReportAppService _reportAppService;
        private readonly IHostEnvironment _environment;

        public VehicleMaintenanceWorkOrderReportDataDefinitions(IConfiguration configuration,
                                        IHttpContextAccessor httpContextAccessor,
                                        IServiceProvider serviceProvider,
                                        IHostEnvironment environment,
                                        ReportAppService reportAppService)

                    : base(configuration, serviceProvider, httpContextAccessor)
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
            var pdfSetting = new Settings();

            //Set the rendering extension and render the report.
            var pdfRenderingExtension = new PdfRenderingExtension();
            var outputProvider = new MemoryStreamProvider();
            ThisPageReport.Document.Render(pdfRenderingExtension, outputProvider, pdfSetting);

            var memStream = (MemoryStream)outputProvider.GetPrimaryStream().OpenStream();
            return memStream;
        }

        public override async Task<object> LocateDataSource(LocateDataSourceArgs arg)
        {
            var json = string.Empty;
            if (arg.DataSet.Name.Equals("VehicleMaintenanceWorkOrderDataSet"))
            {
                json = await GetVehicleMaintenanceWorkOrderDataSource(arg);
            }
            else if (arg.DataSet.Name.Equals("VehicleMaintenanceWorkOrderLinesDataSet"))
            {
                json = await GetVehicleMaintenanceWorkOrderLinesDataSource(arg);
            }
            return json;
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
            }
            else
            {
                var contentJson = await response.Content.ReadAsStringAsync();
                //var resultJson = JObject.Parse(contentJson)["result"].ToString();
                return contentJson;
            }

            return "[]";
        }

        internal async Task<string> GetVehicleMaintenanceWorkOrderDataSource(LocateDataSourceArgs arg)
        {
            var accessToken = await base.HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var headers = $"headers={{\"Accept\":\"application/json\", \"Authorization\":\"Bearer {accessToken}\"}}";
            var hostApiUrl = Configuration["IdentityServer:Authority"];
            var paramsDic = arg.Parameters.ToDictionary(p => p.Name, p => p.Value);
            var reportParamsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);

            using var client = new HttpClient();
            client.SetBearerToken(accessToken);

            var url = $"{hostApiUrl}/api/services/app/workorder/getworkorderforedit?id={reportParamsDic["EntityId"]}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var contentJson = await response.Content.ReadAsStringAsync();
                //var resultJson = JObject.Parse(contentJson)["result"].ToString();
                return contentJson;
            }

            return "{}";
        }

        #endregion
    }
}
