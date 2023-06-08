﻿using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
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
    }
}