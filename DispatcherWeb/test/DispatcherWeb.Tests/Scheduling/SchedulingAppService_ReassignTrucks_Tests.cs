using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_ReassignTrucks_Tests : SchedulingAppService_Tests_Base
    {
        [Theory]
        [InlineData(1)]
        [InlineData(.5)]
        public async Task Test_ReassignTrucks_should_reassign_trucks_for_current_date(decimal utilization)
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, utilization);

            // Act
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);

            var orderLineTruck1 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine1.Id);
            orderLineTruck1.ShouldNotBeNull();
            orderLineTruck1.IsDeleted.ShouldBeFalse();
            orderLineTruck1.IsDone.ShouldBeTrue();

            var orderLineTruck2 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine2.Id);
            orderLineTruck2.ShouldNotBeNull();
            orderLineTruck2.IsDeleted.ShouldBeFalse();
            orderLineTruck2.IsDone.ShouldBeFalse();
            orderLineTruck2.Utilization.ShouldBe(utilization);
        }

        [Fact]
        public async Task Test_ReassignTrucks_should_not_reassign_same_trucks()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck1 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, .5m);
            var orderLineTruck2 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, .5m);

            // Act
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);

            var updatedOrderLineTruck1 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine1.Id);
            updatedOrderLineTruck1.ShouldNotBeNull();
            updatedOrderLineTruck1.IsDeleted.ShouldBeFalse();
            updatedOrderLineTruck1.IsDone.ShouldBeFalse();

            var updatedOrderLineTruck2 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine2.Id);
            updatedOrderLineTruck2.ShouldNotBeNull();
            updatedOrderLineTruck2.IsDeleted.ShouldBeFalse();
            updatedOrderLineTruck2.IsDone.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_ReassignTrucks_should_reassign_trucks_for_tomorrow_date()
        {
            // Arrange
            DateTime date = Clock.Now.Date.AddDays(1);
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            // Act
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);

            var orderLineTruck1 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine1.Id);
            orderLineTruck1.ShouldNotBeNull();
            orderLineTruck1.IsDeleted.ShouldBeTrue();
            orderLineTruck1.IsDone.ShouldBeFalse();

            var orderLineTruck2 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine2.Id);
            orderLineTruck2.ShouldNotBeNull();
            orderLineTruck2.IsDeleted.ShouldBeFalse();
            orderLineTruck2.IsDone.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_ReassignTrucks_should_reassign_IsDone_trucks()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            await UpdateEntity(orderLineTruck, olt =>
            {
                olt.IsDone = true;
                olt.Utilization = 0;
            });

            // Act
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);

            var orderLineTruck1 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine1.Id);
            orderLineTruck1.ShouldNotBeNull();
            orderLineTruck1.IsDeleted.ShouldBeFalse();
            orderLineTruck1.IsDone.ShouldBeTrue();

            var orderLineTruck2 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine2.Id);
            orderLineTruck2.ShouldNotBeNull();
            orderLineTruck2.IsDeleted.ShouldBeFalse();
            orderLineTruck2.IsDone.ShouldBeFalse();
            orderLineTruck2.Utilization.ShouldBe(1);
        }

        [Fact]
        public async Task Test_ReassignTrucks_should_reassign_active_trucks_with_same_Utilization()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, .5m);

            // Act
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);

            var orderLineTruck1 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine1.Id);
            orderLineTruck1.ShouldNotBeNull();
            orderLineTruck1.IsDeleted.ShouldBeFalse();
            orderLineTruck1.IsDone.ShouldBeTrue();

            var orderLineTruck2 = orderLineTrucks.FirstOrDefault(olt => olt.OrderLineId == orderLine2.Id);
            orderLineTruck2.ShouldNotBeNull();
            orderLineTruck2.IsDeleted.ShouldBeFalse();
            orderLineTruck2.IsDone.ShouldBeFalse();
            orderLineTruck2.Utilization.ShouldBe(.5m);
        }

        [Fact]
        public async Task Test_ReassignTrucks_should_throw_UserFriendlyException_when_Scheduled_is_less_than_trucks_number()
        {
            // Arrange
            DateTime date = Clock.Now.Date.AddDays(1);
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            await UpdateEntity(orderLine2, ol => ol.ScheduledTrucks = 0);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            // Act, Assert
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_ReassignTrucks_should_throw_ArgumentException_when_Order_is_in_past()
        {
            // Arrange
            DateTime date = Clock.Now.Date.AddDays(-1);
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            await UpdateEntity(orderLine2, ol => ol.ScheduledTrucks = 0);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            // Act, Assert
            await _schedulingAppService.ReassignTrucks(new ReassignTrucksInput()
            {
                SourceOrderLineId = orderLine1.Id,
                DestinationOrderLineId = orderLine2.Id,
                TruckIds = new[] { truck.Id },
            }).ShouldThrowAsync(typeof(ArgumentException));
        }
    }
}
