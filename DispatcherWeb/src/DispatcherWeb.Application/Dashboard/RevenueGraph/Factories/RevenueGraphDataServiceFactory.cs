using System;
using Abp.Dependency;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.DataServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;

namespace DispatcherWeb.Dashboard.RevenueGraph.Factories
{
    public static class RevenueGraphDataServiceFactory
    {
        public static IRevenueGraphDataService CreateRevenueGraphDataService(
            IIocResolver iocResolver,
            RevenueGraphDatePeriod revenueGraphDatePeriod,
            RevenueCalculateType revenueCalculateType
        )
        {
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = RevenueGraphDataItemsQueryServiceFactory.CreateRevenueGraphDataItemsQueryService(iocResolver, revenueCalculateType);
            switch (revenueGraphDatePeriod)
            {
                case RevenueGraphDatePeriod.Daily:
                    return new RevenueGraphDataDailyService(revenueGraphDataItemsQueryService);
                case RevenueGraphDatePeriod.Weekly:
                    return new RevenueGraphDataWeeklyService(revenueGraphDataItemsQueryService);
                case RevenueGraphDatePeriod.Monthly:
                    return new RevenueGraphDataMonthlyService(revenueGraphDataItemsQueryService);
                case RevenueGraphDatePeriod.Total:
                    return new RevenueGraphTotalService(revenueGraphDataItemsQueryService);
                default:
                    throw new ArgumentOutOfRangeException($"The unknown RevenueGraphDatePeriod: {revenueGraphDatePeriod}!");
            }
        }

    }
}
