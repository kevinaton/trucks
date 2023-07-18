using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Export.Pdf.Page;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Rendering.IO;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataParameter = GrapeCity.Enterprise.Data.DataEngine.DataProcessing.DataParameter;
using Query = GrapeCity.ActiveReports.PageReportModel.Query;
using PdfSettings = GrapeCity.ActiveReports.Export.Pdf.Page.Settings;
using System.Runtime.CompilerServices;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public abstract class ReportDataDefinitionBase : IReportDataDefinition
    {
        #region public properties

        public IConfiguration Configuration { get; internal set; }

        public IHttpClientFactory HttpClientFactory { get; internal set; }

        public ReportAppService ReportAppService { get; internal set; }

        public PageReport ThisPageReport { get; set; }

        public IHttpContextAccessor HttpContextAccessor { get; internal set; }

        public virtual bool HasTenantsParameter { get; private set; }

        public ILogger Logger { get; private set; }

        public string ReportId =>
            GetType().UnderlyingSystemType.Name.Replace("DataDefinitions", string.Empty);

        #endregion

        #region private variables

        private readonly IHostEnvironment _environment;
        private readonly string _httpClientName = "DispatcherWeb.ReportCenter";
        #endregion

        protected const string _emptyArrayInResult = "{result:[]}";
        protected Dictionary<string, string> _masterDataSourcesRef = new()
        {
            { "TenantsDataSource", "TenantsDataSet" }
        };

        public ReportDataDefinitionBase(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
                                            IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory,
                                            ReportAppService reportAppService, IHostEnvironment environment)
        {
            _environment = environment;

            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            HttpClientFactory = httpClientFactory;
            ReportAppService = reportAppService;
            Logger = loggerFactory.CreateLogger(ReportId);

        }

        public async Task<DataSource> TenantsListDataSource()
        {
            var ds = new DataSource { Name = "TenantsDataSource" };
            ds.ConnectionProperties.DataProvider = "JSON";

            var tenantsJson = await GetTenantsJson();
            if (!string.IsNullOrEmpty(tenantsJson))
                ds.ConnectionProperties.ConnectString = $"jsondata={tenantsJson}";

            return ds;
        }

        public IDataSet TenantsListDataSet()
        {
            DataSet tenantsListDataSet = new()
            {
                Name = _masterDataSourcesRef["TenantsDataSource"]
            };

            Query query = new()
            {
                DataSourceName = "TenantsDataSource",
                CommandText = "$.result.[*]"
            };
            tenantsListDataSet.Query = query;

            //Create individual fields
            Field tenantId = new("tenantId", "tenantId", null);
            Field tenantName = new("tenantName", "tenantName", null);

            //Add fields and filter to the dataset
            tenantsListDataSet.Fields.Add(tenantId);
            tenantsListDataSet.Fields.Add(tenantName);

            return tenantsListDataSet;
        }

        public virtual async Task PostInitialize()
        {
            // Need to remove first the existing datasource for Tenants setup in the report (in the MasterReport)
            ThisPageReport.Report.DataSources.Remove(d => d.Name.Equals("TenantsDataSource"));
            ThisPageReport.Document.PageReport.Report.DataSources.Remove(d => d.Name.Equals("TenantsDataSource"));

            if (!HasTenantsParameter)
            {
                // Remove existing dataset configured for the datasource
                ThisPageReport.Report.DataSets.Remove(d => d.Name.Equals("TenantsDataSet"));
                ThisPageReport.Document.PageReport.Report.DataSets.Remove(d => d.Name.Equals("TenantsDataSet"));

                // Remove/Hide the Tenant parameter 
                ThisPageReport.Report.ReportParameters.Remove(p => p.Name.Equals("TenantId"));
                ThisPageReport.Document.PageReport.Report.ReportParameters.Remove(p => p.Name.Equals("TenantId"));

                // and the labels located in the header component
                var pageHeaderFooters = ThisPageReport.Document.PageReport.Report.Components.Where(c => c is PageHeaderFooter);
                pageHeaderFooters.ToList().ForEach(c => ((PageHeaderFooter)c).Components.HideTenantLabels());
            }
            else
            {
                // Recreate the datasource for Tenants; Dataset is already setup in its query to use the datasource name
                var tenantsListDataSource = await TenantsListDataSource();

                ThisPageReport.Report.DataSources.Add(tenantsListDataSource);
                ThisPageReport.Document.PageReport.Report.DataSources.Add(tenantsListDataSource);
            }
        }

        public async Task Initialize()
        {
            var getReportInfoResult = await ReportAppService.TryGetReport(ReportId);

            if (!getReportInfoResult.Success)
                throw new Exception("Report is not registered.");

            if (!getReportInfoResult.ReportInfo.HasAccess)
                throw new Exception("You do not have access to view this report.");

            var reportsDirPath = new DirectoryInfo($"{_environment.ContentRootPath}\\Reports\\");
            var reportPath = $"{Path.Combine(reportsDirPath.FullName, getReportInfoResult.ReportInfo.Path)}.rdlx";

            ThisPageReport = new PageReport(new FileInfo(reportPath));
            if (ThisPageReport != null)
            {
                await PostInitialize();
            }
        }

        public MemoryStream OpenReportAsPdf(int? entityId)
        {
            ThisPageReport.Document.LocateDataSource += (sender, args) =>
            {
                var dataParams = new List<DataParameter>();
                var reportParams = new List<DataParameter>(args.Parameters);
                if (entityId.HasValue)
                {
                    reportParams.Add(new DataParameter("EntityId", entityId));
                    dataParams.Add(new DataParameter("EntityId", entityId));
                }
                foreach (var p in args.DataSet.Query.QueryParameters)
                {
                    dataParams.Add(new DataParameter(p.Name, p.Value));
                }
                var locateDataSourceArgs = new LocateDataSourceArgs(reportParams, dataParams, args.Report, args.DataSet);
                var dataSource = ThisPageReport.Document.PageReport.Report.DataSources.FirstOrDefault(p => p.Name == args.DataSet.Query.DataSourceName);
                var locateDataSourceResult = LocateDataSource(locateDataSourceArgs).Result;
                args.Data = locateDataSourceResult.DataSourceJson;
            };

            // Provide settings for the rendering output.
            var pdfSetting = new PdfSettings();

            //Set the rendering extension and render the report.
            var pdfRenderingExtension = new PdfRenderingExtension();
            var outputProvider = new MemoryStreamProvider();
            ThisPageReport.Document.Render(pdfRenderingExtension, outputProvider, pdfSetting);

            var memStream = (MemoryStream)outputProvider.GetPrimaryStream().OpenStream();
            return memStream;
        }

        public virtual async Task<(bool IsMasterDataSource, object DataSourceJson)> LocateDataSource(LocateDataSourceArgs arg)
        {
            var isMasterDataSource = IsForTenantsDataSet(arg);
            if (arg.DataSet.Name.Equals("TenantsDataSet"))
            {
                var contentJson = await GetTenantsJson();
                return (isMasterDataSource, contentJson);
            }
            return (isMasterDataSource, _emptyArrayInResult);
        }

        #region private methods

        protected HttpClient GetHttpClient()
        {
             return HttpClientFactory.CreateClient(_httpClientName);
        }

        protected bool IsForTenantsDataSet(LocateDataSourceArgs arg)
        {
            return _masterDataSourcesRef.ContainsValue(arg.DataSet.Name);
        }

        protected async Task<string> ValidateResponse(HttpResponseMessage response, string callerMethodName, string alternativeJsonResult = "")
        {
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                Logger.Log(LogLevel.Error, $"Error: {callerMethodName} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
                return string.IsNullOrEmpty(alternativeJsonResult) ? _emptyArrayInResult : alternativeJsonResult;
            }
            else
            {
                var contentJson = await response.Content.ReadAsStringAsync();
                Logger.Log(LogLevel.Information, $"Success: {callerMethodName} -> {response.ReasonPhrase}; {response.RequestMessage.Method.Method}; {response.RequestMessage.RequestUri.AbsoluteUri};");
                return contentJson;
            }
        }

        private async Task<string> GetTenantsJson()
        {
            var httpClient = GetHttpClient();
            var url = $"/api/services/activeReports/tenantStatisticsReport/GetTenants";
            var response = await httpClient.GetAsync(url);

            var jsonContent = await ValidateResponse(response, Extensions.GetMethodName());
            return jsonContent;
        }

        #endregion
    }
}