using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Receipts.Dto;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Tickets;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatcherWeb.Receipts
{
    public class ReceiptSeeder : ITransientDependency
    {
        protected readonly IUnitOfWorkManager UnitOfWorkManager;
        private readonly ISettingManager _settingManager;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;

        public AspNetZeroAbpSession Session { get; }

        public ReceiptSeeder(

            ISettingManager settingManager,
            IRepository<Order> orderRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IRepository<Tenant> tenantRepository,
            OrderTaxCalculator orderTaxCalculator,
            IUnitOfWorkManager unitOfWorkManager,
            AspNetZeroAbpSession session
            )
        {
            _settingManager = settingManager;
            _orderRepository = orderRepository;
            _receiptRepository = receiptRepository;
            _receiptLineRepository = receiptLineRepository;
            _tenantRepository = tenantRepository;
            _orderTaxCalculator = orderTaxCalculator;
            UnitOfWorkManager = unitOfWorkManager;
            Session = session;
        }

        [Obsolete]
        public async Task SeedReceiptsFromActualAmounts(SeedReceiptsFromActualAmountsInput input)
        {
            //var existingReceipts = _receiptRepository

            List<int> allOrderIds;
            List<int> allTenantIds;
            using (var unitOfWork = UnitOfWorkManager.Begin())
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                if (await _receiptRepository.GetAll()
                    .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId.Value)
                    .AnyAsync(x => !x.IsDeleted))
                {
                    throw new UserFriendlyException("Some receipts already exist");
                }

                allTenantIds = await _tenantRepository.GetAll()
                    .WhereIf(input.TenantId.HasValue, x => x.Id == input.TenantId)
                    .Select(x => x.Id).ToListAsync();

                allOrderIds = await _orderRepository.GetAll()
                    .Where(x => allTenantIds.Contains(x.TenantId))
                    .OrderBy(x => x.TenantId)
                    .ThenBy(x => x.Id)
                    .Select(x => x.Id).ToListAsync();
            }
            var totalOrderCount = allOrderIds.Count;
            while (allOrderIds.Any())
            {
                var idsToProcess = allOrderIds.Take(100).ToArray();
                if (!idsToProcess.Any())
                {
                    break;
                }
                allOrderIds.RemoveRange(0, idsToProcess.Length);
                using (var unitOfWork = UnitOfWorkManager.Begin())
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
                {
                    await SeedReceiptsFromActualAmounts(idsToProcess);
                    await unitOfWork.CompleteAsync();
                }
            }
        }

        private async Task SeedReceiptsFromActualAmounts(params int[] orderIds)
        {
            var orders = await _orderRepository.GetAll()
                .Include(x => x.OrderLines)
                    .ThenInclude(o => o.OfficeAmounts)
                .Include(x => x.OrderLines)
                    .ThenInclude(o => o.Service)
                .Include(x => x.OrderLines)
                    .ThenInclude(o => o.Tickets)
                .Where(x => orderIds.Contains(x.Id) && x.OrderLines.Any(ol => ol.OfficeAmounts.Any(oa => oa.ActualQuantity > 0)))
                .ToListAsync();

            if (!orders.Any())
            {
                return;
            }

            int tenantId = orders.First().TenantId;
            bool allowAddingTickets = await _settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.General.AllowAddingTickets, tenantId);
            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync(tenantId);

            foreach (var order in orders)
            {
                if (order.TenantId != tenantId)
                {
                    tenantId = order.TenantId;
                    allowAddingTickets = await _settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.General.AllowAddingTickets, tenantId);
                    taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync(tenantId);
                }

                var officeIds = order.OrderLines.SelectMany(x => x.OfficeAmounts).Select(x => x.OfficeId).Distinct().ToList();
                foreach (var officeId in officeIds)
                {
                    var receipt = Receipt.FromOrder(order, officeId);
                    _receiptRepository.Insert(receipt);

                    var receiptLineTaxDtos = new List<ReceiptLineTaxDetailsDto>();
                    foreach (var orderLine in order.OrderLines)
                    {
                        var usedTickets = new List<Ticket>();
                        var receiptLine = ReceiptLine.FromOrderLine(orderLine, 0, 0);
                        if (allowAddingTickets)
                        {
                            if (orderLine.IsMaterialPriceOverridden || orderLine.IsFreightPriceOverridden)
                            {
                                //only one ticket is allowed for the overridden values
                                var ticket = orderLine.Tickets.OrderBy(t => t.Id).FirstOrDefault();
                                if (ticket != null)
                                {
                                    var ticketAmount = new TicketQuantityDto(ticket.Quantity, orderLine.Designation, orderLine.MaterialUomId, orderLine.FreightUomId, ticket.UnitOfMeasureId);
                                    //the single allowed ticket hasn't been used up by another receipt line yet
                                    if (ticket.ReceiptLineId == null && ticket.ReceiptLine == null)
                                    {
                                        usedTickets.Add(ticket);
                                        if (orderLine.IsMaterialPriceOverridden)
                                        {
                                            receiptLine.MaterialQuantity = ticketAmount.GetMaterialQuantity();
                                        }
                                        if (orderLine.IsFreightPriceOverridden)
                                        {
                                            receiptLine.FreightQuantity = ticketAmount.GetFreightQuantity();
                                        }
                                    }
                                    else
                                    {
                                        if (orderLine.IsMaterialPriceOverridden)
                                        {
                                            receiptLine.MaterialQuantity = 0;
                                            receiptLine.MaterialAmount = 0;
                                        }
                                        if (orderLine.IsFreightPriceOverridden)
                                        {
                                            receiptLine.FreightQuantity = 0;
                                            receiptLine.FreightAmount = 0;
                                        }
                                    }
                                }
                            }
                            if (!orderLine.IsMaterialPriceOverridden || !orderLine.IsFreightPriceOverridden)
                            {
                                //all new tickets are allowed for non-overridden values
                                var tickets = orderLine.Tickets.Where(t => t.ReceiptLineId == null && t.ReceiptLine == null)
                                    .ToList();
                                usedTickets.AddRange(tickets.Except(usedTickets));
                                if (!orderLine.IsMaterialPriceOverridden)
                                {
                                    receiptLine.MaterialQuantity = tickets.Any() 
                                        ? tickets
                                            .Select(x => new TicketQuantityDto(x.Quantity, orderLine.Designation, orderLine.MaterialUomId, orderLine.FreightUomId, x.UnitOfMeasureId))
                                            .Sum(t => t.GetMaterialQuantity())
                                        : 0; //(decimal?)null;
                                    receiptLine.MaterialAmount = (receiptLine.MaterialQuantity ?? 0) * receiptLine.MaterialRate;
                                }
                                if (!orderLine.IsFreightPriceOverridden)
                                {
                                    receiptLine.FreightQuantity = tickets.Any()
                                        ? tickets
                                            .Select(x => new TicketQuantityDto(x.Quantity, orderLine.Designation, orderLine.MaterialUomId, orderLine.FreightUomId, x.UnitOfMeasureId))
                                            .Sum(t => t.GetFreightQuantity())
                                        : 0; //(decimal?)null;
                                    receiptLine.FreightAmount = (receiptLine.FreightQuantity ?? 0) * receiptLine.FreightRate;
                                }
                            }
                            receiptLine.Receipt = receipt;
                            receiptLineTaxDtos.Add(new ReceiptLineTaxDetailsDto
                            {
                                IsTaxable = orderLine.Service.IsTaxable,
                                FreightAmount = receiptLine.FreightAmount,
                                MaterialAmount = receiptLine.MaterialAmount
                            });
                            _receiptLineRepository.Insert(receiptLine);
                            usedTickets.ForEach(x => x.ReceiptLine = receiptLine);
                        }
                        else
                        {
                            var actualQuantity = orderLine.OfficeAmounts.Where(x => x.OfficeId == officeId).Sum(x => x.ActualQuantity) ?? 0;
                            receiptLine.MaterialQuantity = actualQuantity;
                            receiptLine.FreightQuantity = actualQuantity;
                            receiptLine.Receipt = receipt;
                            receiptLineTaxDtos.Add(new ReceiptLineTaxDetailsDto
                            {
                                IsTaxable = orderLine.Service.IsTaxable,
                                FreightAmount = receiptLine.FreightAmount,
                                MaterialAmount = receiptLine.MaterialAmount
                            });
                            _receiptLineRepository.Insert(receiptLine);
                        }
                    }

                    OrderTaxCalculator.CalculateTotals(taxCalculationType, receipt, receiptLineTaxDtos);
                }
            }
        }
    }
}
