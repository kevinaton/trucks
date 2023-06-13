﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataParameter = GrapeCity.Enterprise.Data.DataEngine.DataProcessing.DataParameter;
using Query = GrapeCity.ActiveReports.PageReportModel.Query;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public abstract class ReportDataDefinitionBase : IReportDataDefinition
    {
        #region public properties

        public IConfiguration Configuration { get; internal set; }

        public PageReport ThisPageReport { get; set; }

        public IHttpContextAccessor HttpContextAccessor { get; internal set; }

        public virtual bool HasTenantsParameter { get; private set; }

        public ILogger Logger { get; private set; }

        public string ReportId =>
            GetType().UnderlyingSystemType.Name.Replace("DataDefinitions", string.Empty);

        #endregion

        protected const string _emptyArrayInResult = "{result:[]}";
        protected Dictionary<string, string> _masterDataSourcesRef = new()
        {
            { "TenantsDataSource", "TenantsDataSet" }
        };

        public ReportDataDefinitionBase(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;

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

        /// <summary>
        /// Should be called after when PageReport has been instantiated and loaded with the report.
        /// </summary>
        public virtual async Task Initialize()
        {
            if (ThisPageReport == null)
                return;

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

        public virtual MemoryStream OpenReportAsPdf(int? entityId)
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

            return null;
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

        protected bool IsForTenantsDataSet(LocateDataSourceArgs arg)
        {
            return _masterDataSourcesRef.ContainsValue(arg.DataSet.Name);
        }

        private async Task<string> GetTenantsJson()
        {
            var hostApiUrl = Configuration["IdentityServer:Authority"];
            var url = $"{hostApiUrl}/api/services/activeReports/tenantStatisticsReport/GetTenants";
            var accessToken = await HttpContextAccessor.HttpContext.GetTokenAsync("access_token");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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

            return _emptyArrayInResult;
        }

        #endregion
    }
}