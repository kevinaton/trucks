using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports.Web.Viewer;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions
{
    public class VehicleMaintenanceWorkOrderReportDataDefinitions : ReportDataDefinitionBase
    {
        public VehicleMaintenanceWorkOrderReportDataDefinitions(IHostEnvironment environment,
                                        ReportAppService reportAppService,
                                        IConfiguration configuration,
                                        IHttpContextAccessor httpContextAccessor,
                                        IHttpClientFactory httpClientFactory,
                                        ILoggerFactory loggerFactory)

                    : base(configuration, httpContextAccessor, httpClientFactory, loggerFactory, reportAppService, environment)
        {
        }

        public override bool HasTenantsParameter => false;

        public override async Task PostInitialize()
        {
            ThisPageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderDataSource");
            ThisPageReport.Document.PageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderDataSource");
            ThisPageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderLinesDataSource");
            ThisPageReport.Document.PageReport.Report.DataSources.ResetDataSourceConnectionString("VehicleMaintenanceWorkOrderLinesDataSource");

            await base.PostInitialize();
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
            var httpClient= GetHttpClient();
            var url = $"/api/services/app/workorder/GetWorkOrderLines?id={entityId}";
            var response = await httpClient.GetAsync(url);
            var jsonContent = await ValidateResponse(response, Extensions.GetMethodName(), "[]");
            return jsonContent;
        }

        internal async Task<string> GetVehicleMaintenanceWorkOrderDataSource(LocateDataSourceArgs arg)
        {
            var reportParamsDic = arg.ReportParameters.ToDictionary(p => p.Name, p => p.Value);
            var httpClient= GetHttpClient();
            var url = $"/api/services/app/workorder/getworkorderforedit?id={reportParamsDic["EntityId"]}";
            var response = await httpClient.GetAsync(url);
            var jsonContent = await ValidateResponse(response, Extensions.GetMethodName(), "{}");
            return jsonContent;
        }

        #endregion
    }
}
