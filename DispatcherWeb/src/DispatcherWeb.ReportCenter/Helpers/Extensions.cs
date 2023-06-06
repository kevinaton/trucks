using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions;
using System;
using GrapeCity.ActiveReports.PageReportModel;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GrapeCity.BI.Data.DataProviders;
using System.Collections.Generic;

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

        public static KeyValuePair<string, object> ToKeyValuePair(this DataParameter dataParam)
        {
            return new KeyValuePair<string, object>(dataParam.ParameterName, dataParam.Value);
        }
    }
}