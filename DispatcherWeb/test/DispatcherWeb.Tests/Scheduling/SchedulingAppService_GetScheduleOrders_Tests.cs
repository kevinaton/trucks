using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Timing;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Services;
using DispatcherWeb.Locations;
using DispatcherWeb.Tests.TestInfrastructure;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_GetScheduleOrders_Tests : AppTestBase, IAsyncLifetime
    {
        private ISchedulingAppService _schedulingAppService;
        private ISmsSender _smsSender;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            var dispatchingAppService = Resolve<IDispatchingAppService>(new { smsSender = _smsSender });
            _schedulingAppService = Resolve<ISchedulingAppService>(new { smsSender = _smsSender, dispatchingAppService = dispatchingAppService });
            ((DispatcherWebAppServiceBase)_schedulingAppService).Session = CreateSession();
        }

        [Fact]
        public async Task Test_GetScheduleOrders_should_return_correct_Loads_and_MaterialAmount()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            Shift shift = Shift.Shift1;
            var order = await CreateOrderWithOrderLines(date, shift);
            var orderLine1 = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, shift, driver.Id);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLine1.Id, DispatchStatus.Created);
            await CreateDispatch(truck.Id, driver.Id, orderLine1.Id, DispatchStatus.Sent);
            await CreateDispatch(truck.Id, driver.Id, orderLine1.Id, DispatchStatus.Acknowledged);
            await CreateDispatchWithTicket(truck, driver.Id, orderLine1, DispatchStatus.Loaded, 10);
            var dispatch = await CreateDispatchWithTicket(truck, driver.Id, orderLine1, DispatchStatus.Completed, 20);
            dispatch.Loads.Count.ShouldBe(1);
            await UpdateEntity(dispatch.Loads.First(), l => l.DestinationDateTime = Clock.Now);

            // Act
            var pagedResult = await _schedulingAppService.GetScheduleOrders(new GetScheduleOrdersInput()
            {
                Date = date,
                Shift = shift,
                OfficeId = _officeId,
            });

            // Assert
            pagedResult.TotalCount.ShouldBe(2);
            var items = pagedResult.Items.ToList();
            items.Count.ShouldBe(2);
        }

        [Theory]
        [InlineData(DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Loaded)]
        public async Task Test_GetScheduleOrders_should_return_correct_Loads_and_MaterialAmount_for_MultipleLoads_is_true(DispatchStatus currentDispatchStatus)
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            Shift shift = Shift.Shift1;
            var order = await CreateOrderWithOrderLines(date, shift);
            var orderLine1 = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, shift, driver.Id);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            // Completed Load
            var dispatch = await CreateDispatchWithTicket(truck, driver.Id, orderLine1, DispatchStatus.Completed, 20);
            dispatch.Loads.Count.ShouldBe(1);
            await UpdateEntity(dispatch.Loads.First(), l =>
            {
                l.SourceDateTime = Clock.Now;
                l.DestinationDateTime = Clock.Now;
            });

            await UpdateEntity(dispatch, d => d.Status = currentDispatchStatus); // MultipleLoads

            if (currentDispatchStatus == DispatchStatus.Loaded)
            {
                var ticket = await CreateTicket(orderLine1, truck, "2", 30);
                await AddTicketToDispatch(ticket, dispatch);
            }
            else
            {
                Debug.Assert(currentDispatchStatus == DispatchStatus.Acknowledged);
                await CreateLoad(dispatch.Id); // MultipleLoads, empty load
            }

            // Act
            var pagedResult = await _schedulingAppService.GetScheduleOrders(new GetScheduleOrdersInput()
            {
                Date = date,
                Shift = shift,
                OfficeId = _officeId,
            });

            // Assert
            pagedResult.TotalCount.ShouldBe(2);
            var items = pagedResult.Items.ToList();
            items.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Test_GetScheduleOrders_should_sort_OrderLines_by_TimeOnJob_without_Date_part()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            Shift shift = Shift.Shift1;
            var order = await CreateOrderWithOrderLines(date, shift);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            await UpdateEntity(orderLine1, ol => ol.TimeOnJob = date.AddDays(1).AddHours(10));
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            await UpdateEntity(orderLine2, ol => ol.TimeOnJob = date.AddHours(11));

            // Act
            var pagedResult = await _schedulingAppService.GetScheduleOrders(new GetScheduleOrdersInput()
            {
                Date = date,
                Shift = shift,
                OfficeId = _officeId,
                Sorting = "Time",
            });

            // Assert
            pagedResult.TotalCount.ShouldBe(2);
            var items = pagedResult.Items.ToList();
            items.Count.ShouldBe(2);
            items[0].Id.ShouldBe(orderLine1.Id);
            items[1].Id.ShouldBe(orderLine2.Id);
        }

        [Fact]
        public async Task Test_GetScheduleOrders_should_return_OrderLines_for_Date()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine1 = order.OrderLines.First();
            var orderLine2 = order.OrderLines.Last();
            await CreateOrderWithOrderLines(date.AddDays(1));
            await CreateOrderWithOrderLines(date.AddDays(-1));
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            // Act
            var pagedResult = await _schedulingAppService.GetScheduleOrders(new GetScheduleOrdersInput()
            {
                Date = date,
                OfficeId = _officeId,
            });

            // Assert
            pagedResult.TotalCount.ShouldBe(2);
            var items = pagedResult.Items.ToList();
            items.Count.ShouldBe(2);
            var item1 = items[0];
            item1.Id.ShouldBe(orderLine1.Id);
            item1.Date.ShouldBe(date);
            item1.Shift.ShouldBeNull();
            item1.OrderId.ShouldBe(order.Id);
            item1.Priority.ShouldBe(order.Priority);
            item1.OfficeId.ShouldBe(order.LocationId);
            item1.CustomerIsCod.ShouldBeTrue();
            item1.CustomerId.ShouldBe(order.CustomerId);
            item1.CustomerName.ShouldBe(order.Customer.Name);
            item1.Time.ShouldBe(orderLine1.TimeOnJob);
            item1.LoadAtId.ShouldBe(orderLine1.LoadAtId);
            item1.LoadAtName.ShouldBe(orderLine1.LoadAt.Name);
            item1.DeliverToName.ShouldBe(orderLine1.DeliverTo.Name);
            item1.Item.ShouldBe(orderLine1.Service.Service1);
            item1.MaterialUom.ShouldBe("Hours");
            item1.MaterialQuantity.ShouldBe(orderLine1.MaterialQuantity);
            item1.NumberOfTrucks.ShouldBe(orderLine1.NumberOfTrucks);
            item1.IsClosed.ShouldBe(orderLine1.IsComplete);
            item1.IsShared.ShouldBeFalse();
            item1.SharedOfficeIds.ShouldBeEmpty();
            item1.Utilization.ShouldBe(1);
            item1.Trucks.Count.ShouldBe(1);
            item1.Trucks.First().Id.ShouldBe(orderLineTruck.Id);
        }

        [Fact]
        public async Task Test_GetScheduleOrders_should_return_rows_with_0_MaterialQuantity_and_not_0_NumberOfTrucks()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            await UsingDbContextAsync(async context =>
            {
                var ol = new OrderLine()
                {
                    TenantId = 1,
                    OrderId = order.Id,
                    LineNumber = 3,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 0,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    ServiceId = order.OrderLines.First().ServiceId,
                    LoadAtId = order.OrderLines.First().LoadAtId,
                    MaterialUomId = 1,
                    NumberOfTrucks = 2,
                };
                await context.OrderLines.AddAsync(ol);
                ol = new OrderLine()
                {
                    TenantId = 1,
                    OrderId = order.Id,
                    LineNumber = 3,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 0,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    ServiceId = order.OrderLines.First().ServiceId,
                    LoadAtId = order.OrderLines.First().LoadAtId,
                    MaterialUomId = 1,
                    NumberOfTrucks = 0,
                };
                await context.OrderLines.AddAsync(ol);
            });

            // Act
            var pagedResult = await _schedulingAppService.GetScheduleOrders(new GetScheduleOrdersInput()
            {
                Date = date,
                OfficeId = _officeId,
            });

            // Assert
            pagedResult.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task Test_GetScheduleOrders_should_return_OrderLines_for_Date_and_DefaultOffice_when_AllowMultiOfficeFeature_is_false()
        {
            // Arrange
            ((ApplicationService)_schedulingAppService).SubstituteAllowMultiOfficeFeature(false);
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine1 = order.OrderLines.First();
            var orderLine2 = order.OrderLines.Last();
            await CreateOrderWithOrderLines(date.AddDays(1));
            await CreateOrderWithOrderLines(date.AddDays(-1));
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            // Act
            var pagedResult = await _schedulingAppService.GetScheduleOrders(new GetScheduleOrdersInput()
            {
                Date = date,
            });

            // Assert
            pagedResult.TotalCount.ShouldBe(2);
        }


        private async Task<Dispatch> CreateDispatchWithTicket(Truck truck, int driverId, OrderLine orderLine, DispatchStatus dispatchStatus, decimal quantity)
        {
            return await AddTicketToDispatch(
                await CreateTicket(orderLine, truck, quantity: quantity),
                await CreateDispatch(truck.Id, driverId, orderLine.Id, dispatchStatus)
            );
        }

        private async Task<Dispatch> AddTicketToDispatch(Ticket ticket, Dispatch dispatch)
        {
            return await UsingDbContextAsync(async context =>
            {
                var dispatchEntity = await context.Dispatches
                    .Include(d => d.Loads)
                        .ThenInclude(d => d.Tickets)
                    .FirstAsync(d => d.Id == dispatch.Id);
                var ticketEntity = await context.Tickets.FindAsync(ticket.Id);
                if (dispatchEntity.Loads == null)
                {
                    dispatchEntity.Loads = new List<Load>();
                }
                Load load = dispatchEntity.Loads.FirstOrDefault(l => l.Tickets == null) ?? new Load();
                load.SourceDateTime = Clock.Now;
                load.Tickets.Add(ticketEntity);
                dispatchEntity.Loads.Add(load);
                return dispatchEntity;
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
