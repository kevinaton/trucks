using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices
{
    public class RevenueGraphByOfficeAmountDataItemsQueryService : IRevenueGraphByOfficeAmountDataItemsQueryService
    {
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        public readonly AspNetZeroAbpSession _session;

        public RevenueGraphByOfficeAmountDataItemsQueryService(
            IRepository<ReceiptLine> receiptLineRepository,
            AspNetZeroAbpSession session
        )
        {
            _receiptLineRepository = receiptLineRepository;
            _session = session;
        }

        public async Task<IEnumerable<RevenueGraphDataItem>> GetRevenueGraphDataItemsAsync(PeriodInput input)
        {
            return await _receiptLineRepository.GetAll()
                    .Where(ol => ol.Receipt.Order.DeliveryDate >= input.PeriodBegin)
                    .Where(ol => ol.Receipt.Order.DeliveryDate <= input.PeriodEnd)
                    .Where(ol => ol.Receipt.OfficeId == _session.OfficeId)
                    .Select(ol => new RevenueGraphDataItem
                    {
                        DeliveryDate = ol.Receipt.Order.DeliveryDate,
                        FreightPricePerUnit = ol.FreightRate,
                        MaterialPricePerUnit = ol.MaterialRate,
                        MaterialQuantity = ol.MaterialQuantity == null ? 0 : ol.MaterialQuantity.Value,
                        FreightQuantity = ol.FreightQuantity == null ? 0 : ol.FreightQuantity.Value,
                        OrderLineId = ol.Id,
                        FreightPriceOriginal = ol.OrderLine.FreightPrice,
                        MaterialPriceOriginal = ol.OrderLine.MaterialPrice,
                        IsFreightPriceOverridden = ol.OrderLine.IsFreightPriceOverridden,
                        IsMaterialPriceOverridden = ol.OrderLine.IsMaterialPriceOverridden
                    }).ToListAsync()
                ;

        }
    }

}
