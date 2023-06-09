using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Web.Viewer;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        public override bool HasTenantsParameter => true;

        public override async Task Initialize()
        {
            var getReportInfoResult = await _reportAppService.TryGetReport(ReportId);

            if (!getReportInfoResult.Success)
                throw new Exception("Report is not registered.");

            if (!getReportInfoResult.ReportInfo.HasAccess)
                throw new Exception("You do not have access to view this report.");

            var reportsDirPath = new DirectoryInfo($"{_environment.ContentRootPath}\\Reports\\");
            var reportPath = $"{Path.Combine(reportsDirPath.FullName, getReportInfoResult.ReportInfo.Path)}.rdlx";

            ThisPageReport = new PageReport(new FileInfo(reportPath));
            ThisPageReport.Report.DataSources.ResetDataSourceConnectionString("TenantsStatisticsDataSource");
            ThisPageReport.Document.PageReport.Report.DataSources.ResetDataSourceConnectionString("TenantsStatisticsDataSource");

            await base.Initialize();
        }

        public override async Task<object> LocateDataSource(LocateDataSourceArgs arg)
        {
            if (arg.DataSet.Name.Equals("TenantStatisticsDataSet"))
            {
                var accessToken = await base.HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var headers = $"headers={{\"Accept\":\"application/json\", \"Authorization\":\"Bearer {accessToken}\"}}";
                var hostApiUrl = Configuration["IdentityServer:Authority"];
                var paramsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);

                using var client = new HttpClient();
                client.SetBearerToken(accessToken);

                var tenantId = paramsDic.ContainsKey("TenantId") ? paramsDic["TenantId"] : null;
                var url = $"{hostApiUrl}/api/services/activeReports/tenantStatisticsReport/getTenantStatistics?tenantId={tenantId}&startDate={paramsDic["StartDate"]:o}&endDate={paramsDic["EndDate"]:o}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var contentJson = await response.Content.ReadAsStringAsync();
                    return contentJson;
                }
            }
            return string.Empty;
        }
    }
}
