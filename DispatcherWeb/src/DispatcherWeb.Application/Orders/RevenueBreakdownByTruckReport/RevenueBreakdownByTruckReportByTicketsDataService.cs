using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public class RevenueBreakdownByTruckReportByTicketsDataService : IRevenueBreakdownByTruckReportByTicketsDataService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;

        public RevenueBreakdownByTruckReportByTicketsDataService(
            IRepository<OrderLine> orderLineRepository
        )
        {
            _orderLineRepository = orderLineRepository;
        }

        public async Task<List<RevenueBreakdownByTruckItem>> GetRevenueBreakdownItems(RevenueBreakdownByTruckReportInput input)
        {
            var items = await _orderLineRepository.GetAll()
                .Where(ol => ol.Tickets.Any(t => t.TruckId != null))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), ol => ol.Order.Shift.HasValue && input.Shifts.Contains(ol.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), ol => !ol.Order.Shift.HasValue || input.Shifts.Contains(ol.Order.Shift.Value))
                .Where(ol => ol.Order.DeliveryDate >= input.DeliveryDateBegin)
                .Where(ol => ol.Order.DeliveryDate <= input.DeliveryDateEnd)
                .SelectMany(ol => ol.Tickets)
                .WhereIf(input.OfficeId.HasValue, t => t.OfficeId == input.OfficeId.Value)
                .WhereIf(!input.TruckIds.IsNullOrEmpty(), t => input.TruckIds.Contains(t.TruckId.Value))
                .Select(t => new RevenueBreakdownByTruckItemRaw
                {
                    DeliveryDate = t.OrderLine.Order.DeliveryDate,
                    Shift = t.OrderLine.Order.Shift,
                    TruckId = t.TruckId,
                    TruckCode = t.Truck.TruckCode,
                    //ReceiptFreightTotal = t.ReceiptLine == null ? (decimal?)null : t.ReceiptLine.FreightAmount,
                    //ReceiptMaterialTotal = t.ReceiptLine == null ? (decimal?)null : t.ReceiptLine.MaterialAmount,
                    FreightPricePerUnit = t.OrderLine.FreightPricePerUnit,
                    MaterialPricePerUnit = t.OrderLine.MaterialPricePerUnit,
                    ActualQuantity = t.Quantity,
                    Designation = t.OrderLine.Designation,
                    MaterialUomId = t.OrderLine.MaterialUomId,
                    FreightUomId = t.OrderLine.FreightUomId,
                    TicketUomId = t.UnitOfMeasureId,
                    FreightPriceOriginal = t.OrderLine.FreightPrice,
                    MaterialPriceOriginal = t.OrderLine.MaterialPrice,
                    IsFreightPriceOverridden = t.OrderLine.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = t.OrderLine.IsMaterialPriceOverridden,
                    OrderLineTicketsSum = t.OrderLine == null ? 0 : t.OrderLine.Tickets.Select(x => x.Quantity).Sum(),
                    FuelSurcharge = t.FuelSurcharge
                })
                .ToListAsync();

            return items
                .GroupBy(olt => new { olt.DeliveryDate, olt.Shift, olt.TruckId, olt.TruckCode })
                .Select(g => new RevenueBreakdownByTruckItem
                {
                    DeliveryDate = g.Key.DeliveryDate,
                    Shift = g.Key.Shift,
                    Truck = g.Key.TruckCode,
                    TruckId = g.Key.TruckId,
                    MaterialRevenue = g.Sum(t => t.IsMaterialPriceOverridden ? decimal.Round(t.MaterialPriceOriginal * t.PercentQtyForTicket, 2) : decimal.Round((t.MaterialPricePerUnit ?? 0) * t.ActualMaterialQuantity, 2)),
                    FreightRevenue = g.Sum(t => t.IsFreightPriceOverridden ? decimal.Round(t.FreightPriceOriginal * t.PercentQtyForTicket, 2) : decimal.Round((t.FreightPricePerUnit ?? 0) * t.ActualFreightQuantity, 2)),
                    FuelSurcharge = g.Sum(t => t.FuelSurcharge)
                })
                .ToList();
        }
    }
}
