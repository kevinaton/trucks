using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Orders.TaxDetails;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders
{
    public class OrderTaxCalculator : ITransientDependency
    {
        protected readonly IUnitOfWorkManager UnitOfWorkManager;
        private readonly ISettingManager _settingManager;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;

        public OrderTaxCalculator(
            ISettingManager settingManager,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _settingManager = settingManager;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _receiptRepository = receiptRepository;
            _receiptLineRepository = receiptLineRepository;
            UnitOfWorkManager = unitOfWorkManager;
        }

        public async Task<IOrderTaxDetails> CalculateTotalsForReceiptLineAsync(int receiptLineId)
        {
            var receiptDetails = await _receiptLineRepository.GetAll()
                .Where(x => x.Id == receiptLineId)
                .Select(x => new { x.ReceiptId })
                .FirstOrDefaultAsync();

            return await CalculateReceiptTotalsAsync(receiptDetails.ReceiptId);
        }

        public async Task<IOrderTaxDetails> CalculateReceiptTotalsAsync(int receiptId)
        {
            var receipt = await _receiptRepository.GetAll()
                .Where(x => x.Id == receiptId)
                .FirstOrDefaultAsync();

            var receiptLines = await _receiptLineRepository.GetAll()
                .Where(x => x.ReceiptId == receiptId)
                .Select(x => new OrderLineTaxDetailsDto
                {
                    IsTaxable = x.Service.IsTaxable,
                    FreightPrice = x.FreightAmount,
                    MaterialPrice = x.MaterialAmount
                })
                .ToListAsync();

            await CalculateTotalsAsync(receipt, receiptLines);

            return receipt;
        }

        public async Task<IOrderTaxDetailsWithActualAmounts> CalculateTotalsForOrderLineAsync(int orderLineId)
        {
            var orderDetails = await _orderLineRepository.GetAll()
                .Where(x => x.Id == orderLineId)
                .Select(x => new { x.OrderId })
                .FirstOrDefaultAsync();

            return await CalculateTotalsAsync(orderDetails.OrderId);
        }

        public async Task<IOrderTaxDetailsWithActualAmounts> CalculateTotalsAsync(int orderId)
        {
            var order = await _orderRepository.GetAll()
                .Where(x => x.Id == orderId)
                .FirstOrDefaultAsync();

            var orderLinesRaw = await _orderLineRepository.GetAll()
                .Include(x => x.Tickets)
                .Include(x => x.OfficeAmounts) //todo change to receipts?
                .Include(x => x.SharedOrderLines)
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            await UpdateHasAllActualAmountsAsync(order, orderLinesRaw);

            var orderLineDtos = await _orderLineRepository.GetAll()
                .Where(x => x.OrderId == orderId)
                .Select(x => new OrderLineTaxDetailsDto
                {
                    IsTaxable = x.Service.IsTaxable,
                    FreightPrice = x.FreightPrice,
                    MaterialPrice = x.MaterialPrice
                })
                .ToListAsync();

            await CalculateTotalsAsync(order, orderLineDtos);

            return order;
        }

        private async Task UpdateHasAllActualAmountsAsync(Order order, List<OrderLine> orderLines)
        {
            foreach (var orderLine in orderLines)
            {
                orderLine.HasAllActualAmounts = true;

                foreach (var officeId in orderLine.SharedOrderLines
                    .Select(x => x.OfficeId)
                    .Union(new[] { order.LocationId })
                    .Distinct().ToList())
                {
                    if (!orderLine.Tickets.Any(x => x.OfficeId == officeId && x.Quantity > 0))
                    {
                        orderLine.HasAllActualAmounts = false;
                        break;
                    }
                }
            }

            order.HasAllActualAmounts = orderLines.Any() && orderLines.All(x => x.HasAllActualAmounts);
        }

        public async Task<UpdateHasAllActualAmountsValuesResult> UpdateHasAllActualAmountsValues()
        {
            List<int> allOrderIds;
            using (var unitOfWork = UnitOfWorkManager.Begin())
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                allOrderIds = _orderRepository.GetAll().Select(x => x.Id).ToList();
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
                    await UpdateHasAllActualAmountsValues(idsToProcess);
                    await unitOfWork.CompleteAsync();
                }
            }
            return new UpdateHasAllActualAmountsValuesResult
            {
                ProcessedOrdersCount = totalOrderCount
            };
        }

        public async Task UpdateHasAllActualAmountsValues(params int[] orderIds)
        {
            var orders = await _orderRepository.GetAll()
                .Include(x => x.OrderLines)
                    .ThenInclude(o => o.Tickets)
                .Include(x => x.OrderLines)
                    .ThenInclude(o => o.OfficeAmounts)
                .Include(x => x.OrderLines)
                    .ThenInclude(o => o.SharedOrderLines)
                .Where(x => orderIds.Contains(x.Id))
                .ToListAsync();

            foreach (var order in orders)
            {
                foreach (var orderLine in order.OrderLines)
                {
                    orderLine.HasAllActualAmounts = true;

                    foreach (var officeId in orderLine.SharedOrderLines
                        .Select(x => x.OfficeId)
                        .Union(new[] { order.LocationId })
                        .Distinct().ToList())
                    {
                        if (!orderLine.Tickets.Any(x => x.OfficeId == officeId && x.Quantity > 0))
                        {
                            orderLine.HasAllActualAmounts = false;
                            break;
                        }
                    }
                }

                order.HasAllActualAmounts = order.OrderLines.Any() && order.OrderLines.All(x => x.HasAllActualAmounts);
            }
        }

        public async Task<TaxCalculationType> GetTaxCalculationTypeAsync(int tenantId)
        {
            return (TaxCalculationType)await _settingManager.GetSettingValueForTenantAsync<int>(AppSettings.Invoice.TaxCalculationType, tenantId);
        }

        public async Task<TaxCalculationType> GetTaxCalculationTypeAsync()
        {
            return (TaxCalculationType)await _settingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);
        }

        public async Task CalculateTotalsAsync(IOrderTaxDetails order, IEnumerable<IOrderLineTaxDetails> orderLines)
        {
            var taxCalculationType = await GetTaxCalculationTypeAsync();

            CalculateTotals(taxCalculationType, order, orderLines);
        }


        public static void CalculateSingleOrderLineTotals(TaxCalculationType taxCalculationType, IOrderLineTaxTotalDetails orderLine, decimal salesTaxRate)
        {
            var freightTotal = Math.Round(orderLine.FreightPrice, 2);
            var materialTotal = Math.Round(orderLine.MaterialPrice, 2);
            var subtotal = materialTotal + freightTotal;
            var taxableTotal = 0M;
            var taxRate = salesTaxRate / 100;
            var salesTax = 0M;
            var orderLineTotal = 0M;

            switch (taxCalculationType)
            {
                case TaxCalculationType.FreightAndMaterialTotal:
                    taxableTotal = materialTotal + freightTotal;
                    break;

                case TaxCalculationType.MaterialLineItemsTotal:
                    taxableTotal = materialTotal > 0 ? materialTotal + freightTotal : 0;
                    break;

                case TaxCalculationType.MaterialTotal:
                    taxableTotal = materialTotal;
                    break;

                case TaxCalculationType.NoCalculation:
                    taxRate = 0;
                    salesTax = 0;
                    break;
            }

            if (!orderLine.IsTaxable || taxableTotal < 0)
            {
                taxableTotal = 0;
            }

            switch (taxCalculationType)
            {
                case TaxCalculationType.FreightAndMaterialTotal:
                case TaxCalculationType.MaterialLineItemsTotal:
                case TaxCalculationType.MaterialTotal:
                    salesTax = Math.Round(taxableTotal * taxRate, 2);
                    orderLineTotal = Math.Round(subtotal + taxableTotal * taxRate, 2);
                    break;

                case TaxCalculationType.NoCalculation:
                    //salesTax = Math.Round(salesTax, 2);
                    //orderLineTotal = Math.Round(subtotal + salesTax, 2);
                    orderLineTotal = subtotal;
                    break;
            }

            //var totalsToCheck = new[] { order.FreightTotal, order.MaterialTotal, order.SalesTax, order.CODTotal };
            //var maxValue = AppConsts.MaxDecimalDatabaseLength;
            //if (totalsToCheck.Any(x => x > maxValue))
            //{
            //    throw new UserFriendlyException("The value is too big", "One or more totals exceeded the maximum allowed value. Please decrease some of the values so that the total doesn't exceed " + maxValue);
            //}

            orderLine.Subtotal = subtotal;
            orderLine.TotalAmount = orderLineTotal;
            orderLine.Tax = salesTax;
        }

        public static void CalculateTotals(TaxCalculationType taxCalculationType, IOrderTaxDetails order, IEnumerable<IOrderLineTaxDetails> orderLines)
        {
            order.FreightTotal = Math.Round(orderLines.Sum(ol => ol.FreightPrice), 2);
            order.MaterialTotal = Math.Round(orderLines.Sum(ol => ol.MaterialPrice), 2);
            //order.FuelSurcharge = Math.Round(order.FreightTotal * (order.FuelSurchargeRate / 100), 2);
            if (order is IOrderTaxDetailsWithActualAmounts orderWithAmounts && orderLines.All(o => o is IOrderLineTaxDetailsWithMultipleActualAmounts))
            {
                orderWithAmounts.HasAllActualAmounts = orderLines.Any() && orderLines.All(x => ((IOrderLineTaxDetailsWithMultipleActualAmounts)x).HasAllActualAmounts);
            }
            var subtotal = order.MaterialTotal + order.FreightTotal; //+ order.FuelSurcharge;
            var taxableTotal = 0M;
            var taxRate = order.SalesTaxRate / 100;

            switch (taxCalculationType)
            {
                case TaxCalculationType.FreightAndMaterialTotal:
                    taxableTotal = Math.Round(orderLines.Where(x => x.IsTaxable).Sum(x => x.MaterialPrice + x.FreightPrice), 2);
                    break;

                case TaxCalculationType.MaterialLineItemsTotal:
                    taxableTotal = Math.Round(orderLines.Where(x => x.MaterialPrice > 0 && x.IsTaxable).Sum(x => x.MaterialPrice + x.FreightPrice), 2);
                    break;

                case TaxCalculationType.MaterialTotal:
                    taxableTotal = Math.Round(orderLines.Where(x => x.IsTaxable).Sum(x => x.MaterialPrice), 2);
                    break;

                case TaxCalculationType.NoCalculation:
                    taxRate = 0;
                    order.SalesTaxRate = 0;
                    break;
            }

            if (taxableTotal < 0)
            {
                taxableTotal = 0;
            }

            switch (taxCalculationType)
            {
                case TaxCalculationType.FreightAndMaterialTotal:
                case TaxCalculationType.MaterialLineItemsTotal:
                case TaxCalculationType.MaterialTotal:
                    order.SalesTax = Math.Round(taxableTotal * taxRate, 2);
                    order.CODTotal = Math.Round(subtotal + taxableTotal * taxRate, 2);
                    break;

                case TaxCalculationType.NoCalculation:
                    order.SalesTax = Math.Round(order.SalesTax, 2);
                    order.CODTotal = Math.Round(subtotal + order.SalesTax, 2);
                    break;
            }

            var totalsToCheck = new[] { order.FreightTotal, order.MaterialTotal, order.SalesTax, order.CODTotal };
            var maxValue = AppConsts.MaxDecimalDatabaseLength;
            if (totalsToCheck.Any(x => x > maxValue))
            {
                throw new UserFriendlyException("The value is too big", "One or more totals exceeded the maximum allowed value. Please decrease some of the values so that the total doesn't exceed " + maxValue);
            }
        }
    }
}
