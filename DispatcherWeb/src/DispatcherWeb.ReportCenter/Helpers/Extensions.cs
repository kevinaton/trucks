using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions;
using System;
using GrapeCity.ActiveReports.PageReportModel;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GrapeCity.BI.Data.DataProviders;
using System.Collections.Generic;
using GrapeCity.Enterprise.Data.Expressions;

namespace DispatcherWeb.ReportCenter.Helpers
{
    public static class Extensions
    {
        public static IReportDataDefinition Identify(this IServiceProvider serviceProvider, string reportId)
        {
            if (reportId.Equals("TenantStatisticsReport", StringComparison.InvariantCulture))
                return serviceProvider.GetRequiredService<TenantStatisticsReportDataDefinitions>();

            else if (reportId.Equals("VehicleMaintenanceWorkOrderReport", StringComparison.InvariantCulture))
                return serviceProvider.GetService<VehicleMaintenanceWorkOrderReportDataDefinitions>();

            throw new Exception("Report is not registered.");
        }

        public static int Remove(this ReportParameterCollection collection, Func<ReportParameter, bool> predicate)
        {
            var toRemove = collection.FirstOrDefault(predicate);
            if (toRemove != null) collection.Remove(toRemove);
            return collection.Count;
        }

        public static int Remove(this DataSourceCollection dataSources, Func<DataSource, bool> predicate)
        {
            var toRemove = dataSources.FirstOrDefault(predicate);
            if (toRemove != null) dataSources.Remove(toRemove);
            return dataSources.Count;
        }

        public static int Remove(this DataSetCollection dataSets, Func<IDataSet, bool> predicate)
        {
            var toRemove = dataSets.FirstOrDefault(predicate);
            if (toRemove != null) dataSets.Remove(toRemove);
            return dataSets.Count;
        }

        public static KeyValuePair<string, object> ToKeyValuePair(this DataParameter dataParam)
        {
            return new KeyValuePair<string, object>(dataParam.ParameterName, dataParam.Value);
        }

        public static bool TryAddDataSource(this DataSourceCollection dataSources, DataSource dataSource)
        {
            if (dataSources.Any(d => d.Name.Equals(dataSource.Name)))
                return false;

            dataSources.Add(dataSource);
            return true;
        }

        public static bool TryAddDataSet(this DataSetCollection dataSets, IDataSet dataSet)
        {
            if (dataSets.Any(d => d.Name.Equals(dataSet.Name)))
                return false;

            dataSets.Add(dataSet);
            return true;
        }

        public static bool TryAddReportParameter(this ReportParameterCollection parameters, ReportParameter parameter)
        {
            if (parameters.Any(d => d.Name.Equals(parameter.Name)))
                return false;

            parameters.Add(parameter);
            return true;
        }

        public static void HideTenantLabels(this ReportComponentCollection components)
        {
            var hiddenVisibility = new Visibility()
            {
                Hidden = ExpressionInfo.Parse("true", ExpressionResultType.Boolean)
            };
            var txtTenantInMaster = (TextBox)components.FirstOrDefault(c => c is TextBox box && box.Name.Equals("txtTenantInMaster"));
            if (txtTenantInMaster != null) txtTenantInMaster.Visibility = hiddenVisibility;
            var lblTenantInMaster = (TextBox)components.FirstOrDefault(c => c is TextBox box && box.Name.Equals("lblTenantInMaster"));
            if (lblTenantInMaster != null) lblTenantInMaster.Visibility = hiddenVisibility;
        }

        public static void ResetDataSourceConnectionString(this DataSourceCollection dataSources, string dataSourceName)
        {
            var tenantStatisticsDataSource = dataSources.FirstOrDefault(d => d.Name.Equals(dataSourceName));
            if (tenantStatisticsDataSource != null)
            {
                tenantStatisticsDataSource.ConnectionProperties.ConnectString = "jsondoc=";
            }
        }
    }
}