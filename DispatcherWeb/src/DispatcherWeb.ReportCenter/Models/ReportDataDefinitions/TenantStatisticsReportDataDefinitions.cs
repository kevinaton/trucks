using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Web.Viewer;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using HttpClient = System.Net.Http.HttpClient;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions
{
    public class TenantStatisticsReportDataDefinitions : ReportDataDefinitionBase
    {
        private readonly ReportAppService _reportAppService;
        private readonly IHostEnvironment _environment;

        public TenantStatisticsReportDataDefinitions(IConfiguration configuration,
                                        IHttpContextAccessor httpContextAccessor,
                                        IServiceProvider serviceProvider,
                                        IHostEnvironment environment,
                                        ReportAppService reportAppService)

                    : base(configuration, serviceProvider, httpContextAccessor)
        {
            _reportAppService = reportAppService;
            _environment = environment;
        }

        public override async Task Initialize()
        {
            var reportId = "TenantStatisticsReport";

            if (!_reportAppService.TryGetReport(reportId, out var reportInf))
                throw new Exception("Report is not registered.");

            if (!reportInf.HasAccess)
                throw new Exception("You do not have access to view this report.");

            var reportsDirPath = new DirectoryInfo($"{_environment.ContentRootPath}\\Reports\\");
            var reportPath = $"{Path.Combine(reportsDirPath.FullName, reportInf.Path)}.rdlx";

            ThisPageReport = new PageReport(new FileInfo(reportPath));

            await base.Initialize();
        }

        public override async Task<object> LocateDataSource(LocateDataSourceArgs arg)
        {
            if (arg.DataSet.Name.Equals("TenantStatisticsDataSet"))
            {
                var accessToken = await base.HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var headers = $"headers={{\"Accept\":\"application/json\", \"Authorization\":\"Bearer {accessToken}\"}}";
                var hostApiUrl = Configuration["IdentityServer:Authority"];
                var paramsDic = arg.Parameters.ToDictionary(p => p.Name, p => p.Value);

                using var client = new HttpClient();
                client.SetBearerToken(accessToken);

                var url = $"{hostApiUrl}/api/services/activeReports/tenantStatisticsReport/getTenantStatistics?tenantId={paramsDic["tenantId"]}&startDate={paramsDic["startDate"]:o}&endDate={paramsDic["endDate"]:o}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var contentJson = await response.Content.ReadAsStringAsync();
                    var resultJson = JObject.Parse(contentJson)["result"].ToString();
                    return resultJson;
                }
            }
            return string.Empty;
        }
    }
}
