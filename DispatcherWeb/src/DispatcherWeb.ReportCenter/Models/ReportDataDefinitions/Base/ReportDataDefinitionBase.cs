using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.ReportCenter.Helpers;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using DataParameter = GrapeCity.Enterprise.Data.DataEngine.DataProcessing.DataParameter;
using Query = GrapeCity.ActiveReports.PageReportModel.Query;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public abstract class ReportDataDefinitionBase : IReportDataDefinition
    {
        public IConfiguration Configuration { get; internal set; }

        public PageReport ThisPageReport { get; set; }

        public IServiceProvider ServiceProvider { get; internal set; }

        public IHttpContextAccessor HttpContextAccessor { get; internal set; }

        public virtual bool HasTenantsParameter { get; private set; }

        public string ReportId =>
            GetType().UnderlyingSystemType.Name.Replace("DataDefinitions", string.Empty);

        public ReportDataDefinitionBase(IConfiguration configuration, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            HttpContextAccessor = httpContextAccessor;
        }

        public async Task<DataSource> TenantsListDataSource()
        {
            var ds = new DataSource { Name = "TenantsDataSource" };
            ds.ConnectionProperties.DataProvider = "JSON";

            var hostApiUrl = Configuration["IdentityServer:Authority"];
            var accessToken = await HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var connStrData = $"jsondoc={hostApiUrl}/api/services/activeReports/tenantStatisticsReport/GetTenants";
            var connStrHeaders = $"headers={{\"Accept\":\"application/json\", \"Authorization\":\"Bearer {accessToken}\"}}";

            ds.ConnectionProperties.ConnectString = $"{connStrHeaders};{connStrData}";

            return ds;
        }

        public IDataSet TenantsListDataSet()
        {
            DataSet tenantsListDataSet = new()
            {
                Name = "TenantsDataSet"
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

                ThisPageReport.Report.Body.Components.HideTenantLabels();
                ThisPageReport.Document.PageReport.Report.Body.Components.HideTenantLabels();
            }
            else
            {
                // Recreate the datasource for Tenants; Dataset is already setup in its query to use the datasource name
                var tenantsListDataSource = await TenantsListDataSource();
                ThisPageReport.Report.DataSources.Add(tenantsListDataSource);
                ThisPageReport.Document.PageReport.Report.DataSources.Add(tenantsListDataSource);
            }
        }

        public abstract Task<object> LocateDataSource(LocateDataSourceArgs arg);

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
                args.Data = LocateDataSource(locateDataSourceArgs).Result;
            };

            return null;
        }
    }
}

