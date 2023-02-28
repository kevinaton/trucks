﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Customers;
using DispatcherWeb.Locations;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard.RevenueGraph
{
    public class RevenueGraphDataService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected DateTime _startDate;
        private Office _office;

        public async Task InitializeAsync()
        {
            _office = await CreateOffice();
            _startDate = new DateTime(2019, 3, 1);
        }

        protected async Task CreateOrdersAndOrderLinesAndTickets(DateTime startDate, DateTime endDate)
        {
            var truck = await CreateTruck();
            await UsingDbContextAsync(async context =>
            {
                var customer = context.Customers.FirstOrDefault() ?? new Customer() { TenantId = 1, Name = "Cust" };

                var currentDate = _startDate;
                int i = 0;
                while (currentDate <= endDate)
                {
                    Order order = new Order
                    {
                        TenantId = 1,
                        DeliveryDate = currentDate,
                        Shift = null,
                        Customer = customer,
                        LocationId = _office.Id,
                        SalesTaxRate = 2,
                    };
                    OrderLine orderLine = new OrderLine()
                    {
                        TenantId = 1,
                        LineNumber = 1,
                        Designation = DesignationEnum.FreightAndMaterial,
                        MaterialQuantity = 1,
                        FreightQuantity = 1,
                        FreightPricePerUnit = 2,
                        MaterialPricePerUnit = 3,
                        Service = new Service()
                        {
                            TenantId = 1,
                            Service1 = "sss",
                        },
                        LoadAt = new Location()
                        {
                            TenantId = 1,
                            Name = "Location1",
                        },
                        MaterialUomId = 1,
                        FreightUomId = 1,
                        NumberOfTrucks = 2,
                        ScheduledTrucks = 2,
                    };
                    order.OrderLines.Add(orderLine);

                    Ticket ticket = new Ticket()
                    {
                        TenantId = 1,
                        OfficeId = order.LocationId,
                        TruckId = truck.Id,
                        TruckCode = truck.TruckCode,
                        Customer = order.Customer,
                        Service = orderLine.Service,
                        TicketDateTime = order.DeliveryDate,
                        TicketNumber = $"ticket_{++i:N3}",
                        Quantity = 10,
                        UnitOfMeasureId = 1
                    };
                    orderLine.Tickets = new List<Ticket>() { ticket };

                    await context.Orders.AddAsync(order);

                    currentDate = currentDate.AddDays(1);
                }
            });
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
