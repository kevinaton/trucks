using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_ActivateClosedTrucks_Tests : AppTestBase, IAsyncLifetime
    {
        private ISchedulingAppService _schedulingAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;

            _schedulingAppService = Resolve<ISchedulingAppService>();
            ((DispatcherWebAppServiceBase)_schedulingAppService).Session = CreateSession();
        }

        [Fact]
        public async Task Test_ActivateClosedTrucks_should_activate_closed_trucks_with_1_Utilization()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLines = order.OrderLines.ToList();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLines[0].Id, 0);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);

            // Act
            var result = await _schedulingAppService.ActivateClosedTrucks(new ActivateClosedTrucksInput()
            {
                OrderLineId = orderLines[0].Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            result.ShouldBeTrue();
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDone.ShouldBeFalse();
            updatedOrderLineTruck.Utilization.ShouldBe(1);
        }

        [Fact]
        public async Task Test_ActivateClosedTrucks_should_activate_closed_trucks_with_1_Utilization_when_truck_is_utilized_on_another_shift()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLines = order.OrderLines.ToList();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLines[0].Id, 0);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            var order2 = await CreateOrderWithOrderLines(date, Shift.Shift2);
            var orderLine2 = order2.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 1);

            // Act
            var result = await _schedulingAppService.ActivateClosedTrucks(new ActivateClosedTrucksInput()
            {
                OrderLineId = orderLines[0].Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            result.ShouldBeTrue();
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDone.ShouldBeFalse();
            updatedOrderLineTruck.Utilization.ShouldBe(1);
        }

        [Fact]
        public async Task Test_ActivateClosedTrucks_should_activate_closed_trucks_with_half_Utilization()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLines = order.OrderLines.ToList();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLines[0].Id, 0);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLines[1].Id, .5m);

            // Act
            var result = await _schedulingAppService.ActivateClosedTrucks(new ActivateClosedTrucksInput()
            {
                OrderLineId = orderLines[0].Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            result.ShouldBeTrue();
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDone.ShouldBeFalse();
            updatedOrderLineTruck.Utilization.ShouldBe(.5m);
        }

        [Fact]
        public async Task Test_ActivateClosedTrucks_should_not_activate_closed_trucks_with_0_Utilization()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLines = order.OrderLines.ToList();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLines[0].Id, 0);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLines[1].Id, 1);

            // Act
            var result = await _schedulingAppService.ActivateClosedTrucks(new ActivateClosedTrucksInput()
            {
                OrderLineId = orderLines[0].Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            result.ShouldBeFalse();
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDone.ShouldBeTrue();
            updatedOrderLineTruck.Utilization.ShouldBe(0);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
