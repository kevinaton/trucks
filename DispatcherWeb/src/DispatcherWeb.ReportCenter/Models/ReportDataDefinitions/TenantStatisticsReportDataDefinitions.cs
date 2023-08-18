using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions
{
    public class TenantStatisticsReportDataDefinitions : ReportDataDefinitionBase
    {
        public TenantStatisticsReportDataDefinitions(IHostEnvironment environment,
                                        ReportAppService reportAppService,
                                        IConfiguration configuration,
                                        IHttpContextAccessor httpContextAccessor,
                                        IHttpClientFactory httpClientFactory,
                                        ILoggerFactory loggerFactory)

                    : base(configuration, httpContextAccessor, httpClientFactory, loggerFactory, reportAppService, environment)
        {
        }

        public override bool HasTenantsParameter => true;

        public override async Task PostInitialize()
        {
            ThisPageReport.Report.DataSources.ResetDataSourceConnectionString("TenantsStatisticsDataSource");
            ThisPageReport.Document.PageReport.Report.DataSources.ResetDataSourceConnectionString("TenantsStatisticsDataSource");

            await base.PostInitialize();
        }

        public override async Task<(bool IsMasterDataSource, object DataSourceJson)> LocateDataSource(LocateDataSourceArgs arg)
        {
            var jsonContent = _emptyArrayInResult;
            var (isMasterDataSource, dataSourceJson) = await base.LocateDataSource(arg);
            if (isMasterDataSource)
            {
                return (isMasterDataSource, dataSourceJson);
            }

            if (arg.DataSet.Name.Equals("TenantStatisticsDataSet"))
            {
                var paramsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);
                var tenantId = paramsDic.ContainsKey("TenantId") ? paramsDic["TenantId"] : null;

                var httpClient = GetHttpClient();

                var endDate = paramsDic["EndDate"] ?? paramsDic["StartDate"];
                var url = $"/api/services/activeReports/tenantStatisticsReport/getTenantStatistics?tenantId={tenantId}&startDate={paramsDic["StartDate"]:o}&endDate={endDate:o}";

                var response = await httpClient.GetAsync(url);
                jsonContent = await ValidateResponse(response, Extensions.GetMethodName(), _emptyArrayInResult);
            }

            return (isMasterDataSource, jsonContent);
        }
    }
}
