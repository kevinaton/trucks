using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_MoveTruck_Tests : SchedulingAppService_Tests_Base
    {
        [Theory]
        [InlineData(1)]
        [InlineData(.5)]
        public async Task Test_MoveTruck_should_move_Truck_from_one_OrderLine_to_another(decimal utilization)
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, null, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, utilization);

            // Act
            var result = await _schedulingAppService.MoveTruck(new MoveTruckInput()
            {
                TruckId = truck.Id,
                SourceOrderLineTruckId = orderLineTruck.Id,
                DestinationOrderLineId = orderLine2.Id,
            });

            // Assert
            result.Success.ShouldBeTrue();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);
            orderLineTrucks.First(olt => olt.Id == orderLineTruck.Id).IsDeleted.ShouldBeTrue();
            var createdOrderLineTruck = orderLineTrucks.First(olt => olt.Id != orderLineTruck.Id);
            createdOrderLineTruck.TruckId.ShouldBe(truck.Id);
            createdOrderLineTruck.OrderLineId.ShouldBe(orderLine2.Id);
            createdOrderLineTruck.Utilization.ShouldBe(utilization);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(.5)]
        [InlineData(.3)]
        public async Task Test_MoveTruck_should_activate_closed_truck_Truck_with_IsDone_true_and_set_Utilization_from_moving_Truck(decimal utilization)
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, null, driver.Id);
            var orderLineTruck1 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, utilization);
            var orderLineTruck2 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 0);
            await UpdateEntity(orderLineTruck2, olt => olt.IsDone = true);

            // Act
            var result = await _schedulingAppService.MoveTruck(new MoveTruckInput()
            {
                TruckId = truck.Id,
                SourceOrderLineTruckId = orderLineTruck1.Id,
                DestinationOrderLineId = orderLine2.Id,
            });

            // Assert
            result.Success.ShouldBeTrue();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);
            orderLineTrucks.First(olt => olt.Id == orderLineTruck1.Id).IsDeleted.ShouldBeTrue();
            var updatedOrderLineTruck = orderLineTrucks.First(olt => olt.Id == orderLineTruck2.Id);
            updatedOrderLineTruck.TruckId.ShouldBe(truck.Id);
            updatedOrderLineTruck.OrderLineId.ShouldBe(orderLine2.Id);
            updatedOrderLineTruck.Utilization.ShouldBe(utilization);
            updatedOrderLineTruck.IsDone.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_MoveTruck_should_not_move_Trucks_when_is_is_exists_in_destination_OrderLine()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, null, driver.Id);
            var orderLineTruck1 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, .5m);
            var orderLineTruck2 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, .5m);

            // Act
            var result = await _schedulingAppService.MoveTruck(new MoveTruckInput()
            {
                TruckId = truck.Id,
                SourceOrderLineTruckId = orderLineTruck1.Id,
                DestinationOrderLineId = orderLine2.Id,
            });

            // Assert
            result.Success.ShouldBeFalse();
            result.OrderLineTruckExists.ShouldBeTrue();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);
            orderLineTrucks.First(olt => olt.Id == orderLineTruck1.Id).IsDeleted.ShouldBeFalse();
            var updatedOrderLineTruck = orderLineTrucks.First(olt => olt.Id == orderLineTruck2.Id);
            updatedOrderLineTruck.TruckId.ShouldBe(truck.Id);
            updatedOrderLineTruck.OrderLineId.ShouldBe(orderLine2.Id);
            updatedOrderLineTruck.Utilization.ShouldBe(.5m);
            updatedOrderLineTruck.IsDone.ShouldBeFalse();
        }

    }
}
