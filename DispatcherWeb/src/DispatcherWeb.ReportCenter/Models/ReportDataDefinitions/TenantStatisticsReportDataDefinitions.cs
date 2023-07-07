using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                                        ReportAppService reportAppService,
                                        ILoggerFactory loggerFactory)

                    : base(configuration, serviceProvider, httpContextAccessor, loggerFactory)
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

        public override async Task<(bool IsMasterDataSource, object DataSourceJson)> LocateDataSource(LocateDataSourceArgs arg)
        {
            var contentJson = _emptyArrayInResult;
            var (isMasterDataSource, dataSourceJson) = await base.LocateDataSource(arg);
            if (isMasterDataSource)
            {
                return (isMasterDataSource, dataSourceJson);
            }

            if (arg.DataSet.Name.Equals("TenantStatisticsDataSet"))
            {
                var paramsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);
                var tenantId = paramsDic.ContainsKey("TenantId") ? paramsDic["TenantId"] : null;
                var hostApiUrl = Configuration["IdentityServer:Authority"];
                var accessToken = await HttpContextAccessor.HttpContext.GetTokenAsync("access_token");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var endDate = paramsDic["EndDate"] != null ? paramsDic["EndDate"] : paramsDic["StartDate"];
                var url = $"{hostApiUrl}/api/services/activeReports/tenantStatisticsReport/getTenantStatistics?tenantId={tenantId}&startDate={paramsDic["StartDate"]:o}&endDate={endDate:o}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    Logger.Log(LogLevel.Error, $"Error: {Extensions.GetMethodName()} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
                }
                else
                {
                    contentJson = await response.Content.ReadAsStringAsync();
                    Logger.Log(LogLevel.Information, $"Success: {Extensions.GetMethodName()} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
                }
            }
            return (isMasterDataSource, contentJson);
        }
    }
}
