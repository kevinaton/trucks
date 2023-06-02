using System;
using System.Linq;
using System.Threading.Tasks;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Web.Viewer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Query = GrapeCity.ActiveReports.PageReportModel.Query;

namespace DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base
{
    public abstract class ReportDataDefinitionBase : IReportDataDefinition
    {
        public IConfiguration Configuration { get; internal set; }

        public PageReport ThisPageReport { get; set; }

        public IServiceProvider ServiceProvider { get; internal set; }

        public IHttpContextAccessor HttpContextAccessor { get; internal set; }

        public ReportDataDefinitionBase(IConfiguration configuration, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            HttpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task<DataSource> TenantsListDataSource()
        {
            var ds = new DataSource { Name = "TenantsDataSource" };
            ds.ConnectionProperties.DataProvider = "JSON";

            var hostApiUrl = Configuration["IdentityServer:Authority"];
            var accessToken = await HttpContextAccessor.HttpContext.GetTokenAsync("access_token");

            var connStrData = $"jsondoc={hostApiUrl}/api/services/activeReports/tenantStatisticsReport/GetTenants";
            //var connStrData = $"jsondoc={hostApiUrl}/api/services/app/tenant/GetTenants";
            var connStrHeaders = $"headers={{\"Accept\":\"application/json\", \"Authorization\":\"Bearer {accessToken}\"}}";
            ds.ConnectionProperties.ConnectString = $"{connStrHeaders};{connStrData}";

            return ds;
        }

        /// <summary>
        /// 
        /// </summary>
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

        public int TenantId
        {
            get
            {
                var tenantId = 0;
                var claimsDic = HttpContextAccessor.HttpContext.User.Claims.ToDictionary(c => c.Type, c => c.Value);
                if (claimsDic.TryGetValue("http://www.aspnetboilerplate.com/identity/claims/tenantId", out string id))
                    tenantId = int.Parse(id);
                return tenantId;
            }
        }

        /// <summary>
        /// Should be called after when PageReport has been instantiated and loaded with the report.
        /// </summary>
        public virtual async Task Initialize()
        {
            if (ThisPageReport == null)
                return;

            if (!ThisPageReport.Report.DataSources.Any(d => d.Name.Equals("TenantsDataSource")))
            {
                var tenantsListDataSource = await TenantsListDataSource();
                ThisPageReport.Report.DataSources.Add(tenantsListDataSource);
            }

            if (!ThisPageReport.Report.DataSets.Any(d => d.Name.Equals("TenantsDataSet")))
            {
                var tenantsListDataSet = TenantsListDataSet();
                ThisPageReport.Report.DataSets.Add(tenantsListDataSet);

                if (ThisPageReport.Document.PageReport.Report.ReportParameters.Any(p => p.Name == "TenantId"))
                {
                    var tenantParam = ThisPageReport.Document.PageReport.Report.ReportParameters.FirstOrDefault(p => p.Name == "TenantId");
                    tenantParam.ValidValues.DataSetReference = new DataSetReference()
                    {
                        DataSetName = tenantsListDataSet.Name,
                        LabelField = "tenantName",
                        ValueField = "tenantId"
                    };
                }

                if (ThisPageReport.Report.ReportParameters.Any(p => p.Name == "TenantId"))
                {
                    var tenantParam = ThisPageReport.Report.ReportParameters.FirstOrDefault(p => p.Name == "TenantId");
                    tenantParam.ValidValues.DataSetReference = new DataSetReference()
                    {
                        DataSetName = tenantsListDataSet.Name,
                        LabelField = "tenantName",
                        ValueField = "tenantId"
                    };
                }
            }
        }

        public abstract Task<object> LocateDataSource(LocateDataSourceArgs arg);
    }
}
