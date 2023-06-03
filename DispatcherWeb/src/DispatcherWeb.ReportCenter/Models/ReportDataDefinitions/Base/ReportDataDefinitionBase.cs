using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Web.Viewer;
using GrapeCity.Enterprise.Data.Expressions;
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

            var connStrData = $"jsondoc={hostApiUrl}/api/services/app/dashboard/GetTenants";
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

        /// <summary>
        /// Should be called after when PageReport has been instantiated and loaded with the report.
        /// </summary>
        public virtual async Task Initialize()
        {
            if (ThisPageReport == null)
                return;

            if (HasTenantsParameter)
            {
                if (!ThisPageReport.Report.DataSources.Any(d => d.Name.Equals("TenantsDataSource")))
                {
                    var tenantsListDataSource = await TenantsListDataSource();
                    ThisPageReport.Report.DataSources.Add(tenantsListDataSource);
                }

                if (!ThisPageReport.Report.DataSets.Any(d => d.Name.Equals("TenantsDataSet")))
                {
                    var tenantsListDataSet = TenantsListDataSet();
                    ThisPageReport.Report.DataSets.Add(tenantsListDataSet);

                    #region TenantId Parameter

                    if (!ThisPageReport.Report.ReportParameters.Any(p => p.Name == "TenantId"))
                    {
                        var tenantIdParam = new ReportParameter()
                        {
                            Name = "TenantId",
                            Prompt = "Tenant",
                            Hidden = false,
                            Nullable = true,
                            DataType = ReportParameterDataType.Integer,
                            ValidValues = new ValidValues()
                            {
                                DataSetReference = new DataSetReference()
                                {
                                    DataSetName = tenantsListDataSet.Name,
                                    LabelField = "tenantName",
                                    ValueField = "tenantId"
                                }
                            }
                        };
                        //Add the parameter to the report
                        ThisPageReport.Report.ReportParameters.Insert(0, tenantIdParam);
                    }

                    var tenantParam = ThisPageReport.Report.ReportParameters.FirstOrDefault(p => p.Name == "TenantId");
                    tenantParam.ValidValues.DataSetReference = new DataSetReference()
                    {
                        DataSetName = tenantsListDataSet.Name,
                        LabelField = "tenantName",
                        ValueField = "tenantId"
                    };

                    if (!ThisPageReport.Document.PageReport.Report.ReportParameters.Any(p => p.Name == "TenantId"))
                    {
                    }
                    else
                    {
                        tenantParam = ThisPageReport.Document.PageReport.Report.ReportParameters.FirstOrDefault(p => p.Name == "TenantId");
                        tenantParam.ValidValues.DataSetReference = new DataSetReference()
                        {
                            DataSetName = tenantsListDataSet.Name,
                            LabelField = "tenantName",
                            ValueField = "tenantId"
                        };
                    }

                    #endregion
                }
            }
            else
            {
                var hiddenExpression = new Visibility() { Hidden = ExpressionInfo.Parse("true") };

                var lblTenantInMaster = (TextBox)ThisPageReport.Document.PageReport.Report.Body.Components.FirstOrDefault(c => c is TextBox && ((TextBox)c).Name == "lblTenantInMaster");
                if (lblTenantInMaster != null)
                {
                    lblTenantInMaster.Visibility = hiddenExpression;
                }

                var txtTenantInMaster = (TextBox)ThisPageReport.Document.PageReport.Report.Body.Components.FirstOrDefault(c => c is TextBox && ((TextBox)c).Name == "txtTenantInMaster");
                if (txtTenantInMaster != null)
                {
                    txtTenantInMaster.Visibility = hiddenExpression;
                }

                var tenantParameter = ThisPageReport.Document.PageReport.Report.ReportParameters.FirstOrDefault(p => p.Name == "TenantId");
                if (tenantParameter != null)
                {
                    tenantParameter.Hidden = true;
                }
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

