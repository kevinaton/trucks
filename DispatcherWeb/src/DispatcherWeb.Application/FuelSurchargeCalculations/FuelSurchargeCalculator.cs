﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.DailyFuelCosts;
using DispatcherWeb.FuelSurchargeCalculations.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    public class FuelSurchargeCalculator : DispatcherWebDomainServiceBase, IFuelSurchargeCalculator
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<DailyFuelCost> _dailyFuelCostRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public FuelSurchargeCalculator(
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<DailyFuelCost> dailyFuelCostRepository,
            IRepository<Ticket> ticketRepository
        )
        {
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _dailyFuelCostRepository = dailyFuelCostRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task RecalculateOrderLinesWithTickets(DateTime date)
        {
            var currentFuelCost = await _dailyFuelCostRepository.GetAll()
                .Where(x => x.Date < date)
                .Select(x => new
                {
                    x.Date,
                    x.Cost
                })
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync();

            if (currentFuelCost == null)
            {
                Logger.Warn($"No current fuel cost found for date {date:d}, tenantId {Session.TenantId}, exiting FuelSurchargeCalculator");
                return;
            }

            var nextFuelCost = await _dailyFuelCostRepository.GetAll()
                .Where(x => x.Date > currentFuelCost.Date)
                .Select(x => new
                {
                    x.Date
                })
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync();

            await RecalculateOrderLinesWithTickets(new RecalculateFuelSurchargeInput
            {
                StartDate = currentFuelCost.Date.AddDays(1),
                EndDate = nextFuelCost?.Date
            });
        }

        public async Task RecalculateOrderLinesWithTickets(int orderLineId)
        {
            await RecalculateOrderLinesWithTickets(new RecalculateFuelSurchargeInput
            {
                OrderLineId = orderLineId
            });
        }

        public async Task RecalculateOrderLinesWithTicketsForOrder(int orderId)
        {
            await RecalculateOrderLinesWithTickets(new RecalculateFuelSurchargeInput
            {
                OrderId = orderId
            });
        }

        private async Task RecalculateOrderLinesWithTickets(RecalculateFuelSurchargeInput input)
        {
            input.ValidateInput();

            var orderLineQuery = _orderLineRepository.GetAll()
                .Include(x => x.Tickets)
                .WhereIf(input.StartDate.HasValue, x => x.Order.DeliveryDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, x => x.Order.DeliveryDate <= input.EndDate)
                .WhereIf(input.OrderLineId.HasValue, x => x.Id == input.OrderLineId)
                .WhereIf(input.OrderId.HasValue, x => x.OrderId == input.OrderId);

            var orderLines = await orderLineQuery.ToListAsync();

            var orderIds = orderLines.Select(x => x.OrderId).Distinct().ToList();
            var orderCalculations = await _orderRepository.GetAll()
                .Where(x => orderIds.Contains(x.Id))
                .Select(x => new
                {
                    OrderId = x.Id,
                    x.DeliveryDate,
                    OrderBaseFuelCost = x.BaseFuelCost,
                    Calculation = x.FuelSurchargeCalculation == null ? null : new
                    {
                        x.FuelSurchargeCalculation.BaseFuelCost,
                        x.FuelSurchargeCalculation.Increment,
                        x.FuelSurchargeCalculation.FreightRatePercent,
                        x.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                        x.FuelSurchargeCalculation.Credit
                    }
                })
                .ToListAsync();

            var currentFuelCostDate = input.StartDate.HasValue
                ? input.StartDate.Value
                : input.OrderLineId.HasValue || input.OrderId.HasValue
                    ? orderCalculations.FirstOrDefault()?.DeliveryDate
                    : null;

            if (currentFuelCostDate == null)
            {
                Logger.Warn($"No current fuel cost date for orderLineId '{input.OrderLineId}', orderId '{input.OrderId}' exiting FuelSurchargeCalculator");
                return;
            }

            var currentFuelCost = (await _dailyFuelCostRepository.GetAll()
                .Where(x => x.Date < currentFuelCostDate)
                .Select(x => new
                {
                    x.Date,
                    x.Cost
                })
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync())?.Cost ?? 0;

            var invoicedTicketIds = await orderLineQuery
                .SelectMany(x => x.Tickets)
                .Where(x => x.InvoiceLine != null)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var orderLine in orderLines)
            {
                var orderCalculation = orderCalculations.FirstOrDefault(x => x.OrderId == orderLine.OrderId);
                var calculation = orderCalculation?.Calculation;
                var baseFuelCost = calculation?.CanChangeBaseFuelCost == true ? orderCalculation?.OrderBaseFuelCost ?? 0 : calculation?.BaseFuelCost ?? 0;
                var freightRatePercent = calculation?.FreightRatePercent ?? 0;
                var increment = calculation?.Increment ?? 1;
                var credit = calculation?.Credit == true;
                
                if (!invoicedTicketIds.Any(ticketId => orderLine.Tickets.Any(ticket => ticket.Id == ticketId)))
                {
                    orderLine.FuelSurchargeRate = LimitByCredit(credit, orderLine.FreightPricePerUnit * (currentFuelCost - baseFuelCost) * freightRatePercent / (100 * increment));
                }

                foreach (var ticket in orderLine.Tickets)
                {
                    if (invoicedTicketIds.Contains(ticket.Id))
                    {
                        continue;
                    }
                    var ticketQuantity = new TicketQuantityDto(ticket.Quantity, orderLine.Designation, orderLine.MaterialUomId, orderLine.FreightUomId, ticket.UnitOfMeasureId);
                    var freightAmount = orderLine.IsFreightPriceOverridden ? orderLine.FreightPrice : Math.Round(ticketQuantity.GetFreightQuantity() * orderLine.FreightPricePerUnit ?? 0, 2);
                    ticket.FuelSurcharge = LimitByCredit(credit, Math.Round(freightAmount * ((currentFuelCost - baseFuelCost) * freightRatePercent) / (100 * increment), 2));
                }
            }
        }

        public async Task RecalculateTicket(int ticketId)
        {
            var ticket = await _ticketRepository.GetAll()
                .Include(x => x.InvoiceLine)
                .Where(x => x.Id == ticketId)
                .FirstOrDefaultAsync();

            if (ticket.InvoiceLine != null || ticket.OrderLineId == null)
            {
                return;
            }

            var orderLineData = await _orderLineRepository.GetAll()
                .Where(x => x.Id == ticket.OrderLineId)
                .Select(x => new
                {
                    x.Designation,
                    x.MaterialUomId,
                    x.FreightUomId,
                    x.IsFreightPriceOverridden,
                    x.FreightPrice,
                    x.FreightPricePerUnit,
                    x.Order.DeliveryDate,
                    OrderBaseFuelCost = x.Order.BaseFuelCost,
                    Calculation = x.Order.FuelSurchargeCalculation == null ? null : new
                    {
                        x.Order.FuelSurchargeCalculation.BaseFuelCost,
                        x.Order.FuelSurchargeCalculation.Increment,
                        x.Order.FuelSurchargeCalculation.FreightRatePercent,
                        x.Order.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                        x.Order.FuelSurchargeCalculation.Credit
                    }
                }).FirstOrDefaultAsync();

            var currentFuelCostDate = orderLineData?.DeliveryDate;

            if (currentFuelCostDate == null)
            {
                Logger.Warn("No current fuel cost date for orderLineId " + ticket.OrderLineId + ", exiting FuelSurchargeCalculator");
                return;
            }

            var currentFuelCost = (await _dailyFuelCostRepository.GetAll()
                .Where(x => x.Date < currentFuelCostDate)
                .Select(x => new
                {
                    x.Date,
                    x.Cost
                })
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync())?.Cost ?? 0;

            var calculation = orderLineData?.Calculation;
            var baseFuelCost = calculation?.CanChangeBaseFuelCost == true ? orderLineData?.OrderBaseFuelCost ?? 0 : calculation?.BaseFuelCost ?? 0;
            var freightRatePercent = calculation?.FreightRatePercent ?? 0;
            var increment = calculation?.Increment ?? 1;
            var credit = calculation?.Credit == true;

            var ticketQuantity = new TicketQuantityDto(ticket.Quantity, orderLineData.Designation, orderLineData.MaterialUomId, orderLineData.FreightUomId, ticket.UnitOfMeasureId);
            var freightAmount = orderLineData.IsFreightPriceOverridden ? orderLineData.FreightPrice : Math.Round(ticketQuantity.GetFreightQuantity() * orderLineData.FreightPricePerUnit ?? 0, 2);
            ticket.FuelSurcharge = LimitByCredit(credit, Math.Round(freightAmount * ((currentFuelCost - baseFuelCost) * freightRatePercent) / (100 * increment), 2));
        }

        private static decimal LimitByCredit(bool credit, decimal value)
        {
            if (!credit && value < 0)
            {
                return 0;
            }
            return value;
        }

        private static decimal? LimitByCredit(bool credit, decimal? value)
        {
            if (!credit && value < 0)
            {
                return 0;
            }
            return value;
        }
    }
}
