using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dashboard.Dto;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices
{
    public class RevenueGraphByTicketsDataItemsQueryService : IRevenueGraphByTicketsDataItemsQueryService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        public readonly AspNetZeroAbpSession _session;
        private readonly ISettingManager _settingManager;

        public RevenueGraphByTicketsDataItemsQueryService(
            IRepository<OrderLine> orderLineRepository,
            AspNetZeroAbpSession session,
            ISettingManager settingManager
        )
        {
            _orderLineRepository = orderLineRepository;
            _session = session;
            _settingManager = settingManager;
        }

        public async Task<IEnumerable<RevenueGraphDataItem>> GetRevenueGraphDataItemsAsync(PeriodInput input)
        {
            bool showFuelSurcharge = await _settingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            var timeZone = _settingManager.GetSettingValue(TimingSettingNames.TimeZone);
            var periodBeginUtc = input.PeriodBegin.ConvertTimeZoneFrom(timeZone);
            var periodEndUtc = input.PeriodEnd.ConvertTimeZoneFrom(timeZone).AddDays(1);
            var items = await _orderLineRepository.GetAll()
                    .SelectMany(ol => ol.Tickets)
                    .Where(t => t.TicketDateTime >= periodBeginUtc)
                    .Where(t => t.TicketDateTime < periodEndUtc)
                    .WhereIf(input.TicketType == TicketType.InternalTrucks, t => t.CarrierId == null)
                    .WhereIf(input.TicketType == TicketType.LeaseHaulers, t => t.CarrierId != null)
                    //.Where(t => t.OfficeId == _session.OfficeId)                    
                    .WhereIf(await _settingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices), t => t.OfficeId == _session.OfficeId)
                    .Select(t => new RevenueGraphDataItemFromTickets
                    {
                        DeliveryDate = t.TicketDateTime,
                        FreightPricePerUnit = t.OrderLine.FreightPricePerUnit,
                        MaterialPricePerUnit = t.OrderLine.MaterialPricePerUnit,
                        Designation = t.OrderLine.Designation,
                        MaterialUomId = t.OrderLine.MaterialUomId,
                        FreightUomId = t.OrderLine.FreightUomId,
                        TicketUomId = t.UnitOfMeasureId,
                        Quantity = t.Quantity,
                        OrderLineId = t.OrderLineId,
                        FreightPriceOriginal = t.OrderLine.FreightPrice,
                        MaterialPriceOriginal = t.OrderLine.MaterialPrice,
                        IsFreightPriceOverridden = t.OrderLine.IsFreightPriceOverridden,
                        IsMaterialPriceOverridden = t.OrderLine.IsMaterialPriceOverridden,
                        FuelSurcharge = showFuelSurcharge ? t.FuelSurcharge : 0,
                        CarrierId = t.CarrierId,
                        CustomerName = t.Customer.Name,
                        TruckCode = t.Truck.TruckCode,
                        DriverName = t.Driver == null ? null : t.Driver.FirstName + " " + t.Driver.LastName
                    }).ToListAsync();

            items.ForEach(x => x.DeliveryDate = x.DeliveryDate?.ConvertTimeZoneTo(timeZone).Date);

            var orderLineGroups = items.Where(x => x.OrderLineId.HasValue).GroupBy(x => x.OrderLineId);
            foreach (var group in orderLineGroups.ToList())
            {
                var count = group.Count();
                if (count == 1)
                {
                    continue;
                }
                if (group.First().IsMaterialPriceOverridden)
                {
                    foreach (var item in group)
                    {
                        item.MaterialPriceOriginal /= count;
                    }
                }
                if (group.First().IsFreightPriceOverridden)
                {
                    foreach (var item in group)
                    {
                        item.FreightPriceOriginal /= count;
                    }
                }
            }

            return items;
        }
    }
}
