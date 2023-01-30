using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Dashboard.RevenueGraph.Factories;
using DispatcherWeb.Orders.RevenueAnalysisReport.Dto;

namespace DispatcherWeb.Orders.RevenueAnalysisReport
{
    public class RevenueAnalysisReportAppService : DispatcherWebAppServiceBase, IRevenueAnalysisReportAppService
    {
        private readonly IIocResolver _iocResolver;

        public RevenueAnalysisReportAppService(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_RevenueAnalysis)]
        public async Task<RevenueAnalysisReportOutput> GetRevenueAnalysis(RevenueAnalysisReportInput input)
        {
            var revenueGraphDataItemsQueryService = RevenueGraphDataItemsQueryServiceFactory.CreateRevenueGraphDataItemsQueryService(_iocResolver, RevenueCalculateType.ByTickets);

            var queryInput = new PeriodInput
            {
                PeriodBegin = input.DeliveryDateBegin,
                PeriodEnd = input.DeliveryDateEnd,
                TicketType = input.AnalyzeBy.IsIn(AnalyzeRevenueBy.Truck, AnalyzeRevenueBy.Driver)
                    ? Dashboard.Dto.TicketType.InternalTrucks
                    : Dashboard.Dto.TicketType.Both
            };

            var data = await revenueGraphDataItemsQueryService.GetRevenueGraphDataItemsAsync(queryInput);
            IEnumerable<IGrouping<string, RevenueGraphDataItem>> grouping;

            switch (input.AnalyzeBy)
            {
                case AnalyzeRevenueBy.Driver:
                    grouping = data.GroupBy(x => x.DriverName);
                    break;

                case AnalyzeRevenueBy.Truck:
                    grouping = data.GroupBy(x => x.TruckCode);
                    break;

                case AnalyzeRevenueBy.Customer:
                    grouping = data.GroupBy(x => x.CustomerName);
                    break;

                case AnalyzeRevenueBy.Date:
                    grouping = data.GroupBy(x => x.DeliveryDate?.ToShortDateString());
                    break;

                default:
                    throw new ArgumentException(nameof(input.AnalyzeBy));
            }

            return new RevenueAnalysisReportOutput(grouping
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new RevenueAnalysisReportDataItem
                {
                    AnalysisBy = g.Key,
                    MaterialRevenueValue = g.Sum(olt => olt.IsMaterialPriceOverridden ? decimal.Round(olt.MaterialPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * olt.MaterialQuantity, 2)),
                    FreightRevenueValue = g.Sum(olt => olt.IsFreightPriceOverridden ? decimal.Round(olt.FreightPriceOriginal, 2) : decimal.Round((olt.FreightPricePerUnit ?? 0) * olt.FreightQuantity, 2)),
                    InternalTrucksFuelSurchargeValue = g.Sum(olt => decimal.Round(olt.InternalTruckFuelSurcharge, 2)),
                    LeaseHaulersFuelSurchargeValue = g.Sum(olt => decimal.Round(olt.LeaseHaulerFuelSurcharge, 2))
                })
                .OrderByDescending(x => x.RevenueValue)
                .ToList());
        }

    }
}
