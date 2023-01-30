﻿using System;
using Abp.Dependency;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;

namespace DispatcherWeb.Dashboard.RevenueGraph.Factories
{
    public static class RevenueGraphDataItemsQueryServiceFactory
    {
        public static IRevenueGraphDataItemsQueryService CreateRevenueGraphDataItemsQueryService(
            IIocResolver iocResolver, 
            RevenueCalculateType revenueCalculateType
        )
        {
            switch (revenueCalculateType)
            {
                case RevenueCalculateType.ByTickets:
                    return iocResolver.Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
                case RevenueCalculateType.ByOfficeAmount:
                    return iocResolver.Resolve<IRevenueGraphByOfficeAmountDataItemsQueryService>();
                default:
                    throw new ArgumentOutOfRangeException($"The unknown RevenueCalculateType: {revenueCalculateType}!");
            }
        }
    }
}
