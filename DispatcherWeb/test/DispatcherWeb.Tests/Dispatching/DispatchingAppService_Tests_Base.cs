﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Configuration;
using Abp.Timing;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Locations;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.Tests.TestInfrastructure;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected IDispatchingAppService _dispatchingAppService;
        protected ISmsSender _smsSender;
        protected IAppNotifier _appNotifier;
        protected int _officeId;
        protected ISettingManager _settingManager;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            _appNotifier = Substitute.For<IAppNotifier>();
            _dispatchingAppService = Resolve<IDispatchingAppService>(new { smsSender = _smsSender, appNotifier = _appNotifier });
            ((DispatcherWebAppServiceBase)_dispatchingAppService).Session = CreateSession();
            _settingManager = ((AbpServiceBase)_dispatchingAppService).SubstituteDispatchSettings(DispatchVia.SimplifiedSms);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task<Dispatch> GetDispatch(Guid guid) => await UsingDbContextAsync(async context => await context.Dispatches.Include(d => d.Loads).ThenInclude(l => l.Tickets).Where(d => d.Guid == guid).FirstAsync());

        protected async Task<Ticket> CreateTicketForDispatch(Dispatch dispatch, Order order, Truck truck, OrderLine orderLine)
        {
            var ticketEntity = await UsingDbContextAsync(async context =>
            {
                Load loadEntity = await context.Loads.Include(d => d.Tickets).Where(l => l.DispatchId == dispatch.Id).OrderByDescending(l => l.Id).FirstOrDefaultAsync();
                if (loadEntity == null || loadEntity.Tickets.Any())
                {
                    loadEntity = new Load() { DispatchId = dispatch.Id, SourceDateTime = Clock.Now };
                }
                Ticket ticket = new Ticket()
                {
                    OrderLineId = dispatch.OrderLineId,
                    OfficeId = order.LocationId,
                    TruckId = dispatch.TruckId,
                    TruckCode = truck.TruckCode,
                    CustomerId = order.CustomerId,
                    ServiceId = orderLine.ServiceId,
                    DriverId = dispatch.DriverId,
                    TicketDateTime = order.DeliveryDate,
                    TicketNumber = "111",
                    Quantity = 12.34m,
                    UnitOfMeasureId = 1
                };
                loadEntity.Tickets.Add(ticket);
                context.Update(loadEntity);
                await context.SaveChangesAsync();
                return ticket;
            });
            return ticketEntity;
        }

        protected async Task<Truck> CreateTruckEntity(bool withDriver = false, bool withDriverAssignment = false)
        {
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var office = new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" };
                // Create several trucks so the Id wouldn't be always equal to 1
                var vehicleCategory = await CreateVehicleCategory();
                Truck truck = new Truck()
                {
                    TenantId = 1,
                    TruckCode = "101",
                    Office = office,
                    VehicleCategory = vehicleCategory,
                };
                await context.Trucks.AddAsync(truck);
                truck = new Truck()
                {
                    TenantId = 1,
                    TruckCode = "102",
                    Office = office,
                    VehicleCategory = vehicleCategory,
                };
                await context.Trucks.AddAsync(truck);

                truck = new Truck()
                {
                    TenantId = 1,
                    TruckCode = "001",
                    Office = office,
                    VehicleCategory = vehicleCategory,
                };
                if (withDriver)
                {
                    truck.DefaultDriver = new Driver
                    {
                        TenantId = 1,
                        FirstName = "fn1",
                        LastName = "ln1",
                        CellPhoneNumber = "+15005550055",
                    };
                }
                await context.Trucks.AddAsync(truck);

                return truck;
            });
            return truckEntity;
        }

        protected async Task<Order> CreateOrderAndAssignTruck(DateTime orderDate, Truck truck)
        {
            var orderEntity = await UsingDbContextAsync(async context =>
            {
                Order order = new Order
                {
                    TenantId = 1,
                    DeliveryDate = orderDate,
                    Customer = new Customer() { TenantId = 1, Name = "Cust" },
                    LocationId = _officeId,
                    SalesTaxRate = 2,
                };
                order.OrderLines.Add(new OrderLine()
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    MaterialQuantity = 5,
                    FreightQuantity = 5,
                    FreightPrice = 2 * 5,
                    MaterialPrice = 3 * 5,
                    Service = new Service()
                    {
                        TenantId = 1,
                        Service1 = "sss",
                    },
                    MaterialUomId = 1,
                    FreightUomId = 1
                });
                var orderLine = new OrderLine()
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 20,
                    MaterialPricePerUnit = 30,
                    MaterialQuantity = 2,
                    FreightQuantity = 2,
                    FreightPrice = 20 * 2,
                    MaterialPrice = 30 * 2,
                    Service = new Service()
                    {
                        TenantId = 1,
                        Service1 = "sss",
                    },
                    MaterialUomId = 1,
                    FreightUomId = 1,
                    LoadAt = new Location
                    {
                        Name = "name",
                        StreetAddress = "street",
                        City = "city",
                        State = "state",
                        ZipCode = "zip",
                        CountryCode = "country"
                    },
                };
                orderLine.OrderLineTrucks.Add(new OrderLineTruck()
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    DriverId = truck.DefaultDriverId,
                    Utilization = 1,
                });
                order.OrderLines.Add(orderLine);
                await context.Orders.AddAsync(order);

                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.Office = order.Office;
                await context.SaveChangesAsync();

                return order;
            });
            return orderEntity;
        }
    }
}
