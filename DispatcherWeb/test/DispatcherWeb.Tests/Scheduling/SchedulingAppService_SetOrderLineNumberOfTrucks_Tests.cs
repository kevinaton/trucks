using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_SetOrderLineNumberOfTrucks_Tests : AppTestBase, IAsyncLifetime
    {
        private int _officeId;
        private ISchedulingAppService _schedulingAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            var session = CreateSession();

            var orderLineScheduledTrucksUpdater = Resolve<IOrderLineScheduledTrucksUpdater>();
            _schedulingAppService = Resolve<ISchedulingAppService>(new { orderLineScheduledTrucksUpdater });
            ((DispatcherWebAppServiceBase)_schedulingAppService).Session = session;
            ((DispatcherWebAppServiceBase)orderLineScheduledTrucksUpdater).Session = session;
        }

        [Fact]
        public async Task Test_SetOrderLineNumberOfTrucks_should_update_NumberOfTrucks()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();

            // Act
            var result = await _schedulingAppService.SetOrderLineNumberOfTrucks(new SetOrderLineNumberOfTrucksInput()
            {
                OrderLineId = orderLine.Id,
                NumberOfTrucks = 3,
            });

            // Assert
            result.NumberOfTrucks.ShouldBe(3);
            result.OrderUtilization.ShouldBe(0);
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(orderLine.Id));
            updatedOrderLine.NumberOfTrucks.ShouldBe(3);
        }

        [Fact]
        public async Task Test_SetOrderLineNumberOfTrucks_should_remove_OrderLineTruck_when_setting_NumberOfTrucks_0_and_Quantity_is_0()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            await UpdateEntity(orderLine, ol => ol.MaterialQuantity = 0);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            // Act
            var result = await _schedulingAppService.SetOrderLineNumberOfTrucks(new SetOrderLineNumberOfTrucksInput()
            {
                OrderLineId = orderLine.Id,
                NumberOfTrucks = 0,
            });

            // Assert
            result.NumberOfTrucks.ShouldBe(0);
            result.OrderUtilization.ShouldBe(0);
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(orderLine.Id));
            updatedOrderLine.NumberOfTrucks.ShouldBe(0);
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDeleted.ShouldBeTrue();

        }

        [Fact]
        public async Task Test_SetOrderLineNumberOfTrucks_should_update_NumberOfTrucks_when_OrderLineTruck_exists()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            // Act
            var result = await _schedulingAppService.SetOrderLineNumberOfTrucks(new SetOrderLineNumberOfTrucksInput()
            {
                OrderLineId = orderLine.Id,
                NumberOfTrucks = 1,
            });

            // Assert
            result.NumberOfTrucks.ShouldBe(1);
            result.OrderUtilization.ShouldBe(1);
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(orderLine.Id));
            updatedOrderLine.NumberOfTrucks.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetOrderLineNumberOfTrucks_should_not_throw_UserFriendlyException_when_NumberOfTrucks_is_less_than_Utilization()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var truck2 = await CreateTruck();
            var driver2 = await CreateDriver();
            var orderLineTruck2 = await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine.Id, 1);

            // Act
            var result = await _schedulingAppService.SetOrderLineNumberOfTrucks(new SetOrderLineNumberOfTrucksInput()
            {
                OrderLineId = orderLine.Id,
                NumberOfTrucks = 1,
            });

            // Assert
            result.NumberOfTrucks.ShouldBe(1);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
