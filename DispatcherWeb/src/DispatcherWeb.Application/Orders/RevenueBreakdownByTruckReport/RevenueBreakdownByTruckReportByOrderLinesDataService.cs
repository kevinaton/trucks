using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public class RevenueBreakdownByTruckReportByOrderLinesDataService : IRevenueBreakdownByTruckReportByOrderLinesDataService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;

        public RevenueBreakdownByTruckReportByOrderLinesDataService(
            IRepository<OrderLine> orderLineRepository
        )
        {
            _orderLineRepository = orderLineRepository;
        }

        [UnitOfWork]
        public async Task<List<RevenueBreakdownByTruckItem>> GetRevenueBreakdownItems(RevenueBreakdownByTruckReportInput input)
        {
            var items = await _orderLineRepository.GetAll()
                .WhereIf(input.OfficeId.HasValue, ol => ol.Order.LocationId == input.OfficeId.Value)
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), ol => ol.Order.Shift.HasValue && input.Shifts.Contains(ol.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), ol => !ol.Order.Shift.HasValue || input.Shifts.Contains(ol.Order.Shift.Value))
                .Where(ol => ol.Order.DeliveryDate >= input.DeliveryDateBegin)
                .Where(ol => ol.Order.DeliveryDate <= input.DeliveryDateEnd)
                .SelectMany(ol => ol.OrderLineTrucks)
                .WhereIf(!input.TruckIds.IsNullOrEmpty(), olt => input.TruckIds.Contains(olt.TruckId))
                .Select(olt => new
                {
                    OrderLineTruck = olt,
                    OrderLine = olt.OrderLine,
                    Truck = olt.Truck,
                    ReceiptLines = olt.OrderLine.ReceiptLines.Where(x => x.Receipt.OfficeId == input.OfficeId)
                })
                .Select(olt => new 
                {
                    DeliveryDate = olt.OrderLine.Order.DeliveryDate,
                    Shift = olt.OrderLine.Order.Shift,
                    TruckId = olt.OrderLineTruck.TruckId,
                    TruckCode = olt.Truck.TruckCode,
                    //ReceiptFreightTotal = olt.ReceiptLines.Sum(rl => rl.FreightAmount),
                    //ReceiptMaterialTotal = olt.ReceiptLines.Sum(rl => rl.MaterialAmount),
                    FreightPricePerUnit = olt.OrderLine.FreightPricePerUnit,
                    MaterialPricePerUnit = olt.OrderLine.MaterialPricePerUnit,
                    //ActualMaterialQuantity = olt.OrderLine.OfficeAmounts.FirstOrDefault(oa => oa.OfficeId == input.OfficeId).ActualQuantity ?? 0,
                    ActualMaterialQuantity = olt.OrderLine.MaterialQuantity,
                    ActualFreightQuantity = olt.OrderLine.FreightQuantity,
                    FreightPriceOriginal = olt.OrderLine.FreightPrice,
                    MaterialPriceOriginal = olt.OrderLine.MaterialPrice,
                    IsFreightPriceOverridden = olt.OrderLine.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = olt.OrderLine.IsMaterialPriceOverridden,
                })
                .ToListAsync();

            return items
                .GroupBy(olt => new {olt.DeliveryDate, olt.Shift, olt.TruckId, olt.TruckCode})
                .Select(g => new RevenueBreakdownByTruckItem
                {
                    DeliveryDate = g.Key.DeliveryDate,
                    Shift = g.Key.Shift,
                    Truck = g.Key.TruckCode,
                    TruckId = g.Key.TruckId,
                    MaterialRevenue = g.Sum(olt => olt.IsMaterialPriceOverridden ? decimal.Round(olt.MaterialPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * (olt.ActualMaterialQuantity ?? 0), 2)),
                    FreightRevenue = g.Sum(olt => olt.IsFreightPriceOverridden ? decimal.Round(olt.FreightPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * (olt.ActualFreightQuantity ?? 0), 2)),
                    FuelSurcharge = 0
                })
                .ToList();
        }

    }
}
