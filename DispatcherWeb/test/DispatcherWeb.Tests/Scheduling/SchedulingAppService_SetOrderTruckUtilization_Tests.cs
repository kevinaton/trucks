using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Scheduling.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_SetOrderTruckUtilization_Tests : SchedulingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_SetOrderTruckUtilization_should_change_utilization_for_Truck()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine.Id, 1);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruckEntity = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            await _schedulingAppService.SetOrderTruckUtilization(new OrderLineTruckUtilizationEditDto()
            {
                OrderLineTruckId = orderLineTruckEntity.Id,
                MaxUtilization = 1,
                Utilization = .5m,
            });

            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruckEntity.Id));
            updatedOrderLineTruck.Utilization.ShouldBe(.5m);
            updatedOrderLineTruck.Id.ShouldBe(orderLineTruckEntity.Id);
        }

        [Fact]
        public async Task Test_SetOrderTruckUtilization_should_remove_OrderLineTruck_and_delete_dispatch_when_there_is_sent_Dispatch_and_Utilization_is_zero()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine.Id, 1);
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var orderLineTruckEntity = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);

            // Act
            await _schedulingAppService.SetOrderTruckUtilization(new OrderLineTruckUtilizationEditDto()
            {
                OrderLineTruckId = orderLineTruckEntity.Id,
                MaxUtilization = 1,
                Utilization = 0,
            });

            // Assert
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruckEntity.Id));
            updatedOrderLineTruck.IsDeleted.ShouldBeTrue();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_SetOrderTruckUtilization_should_throw_ApplicationException_when_there_are_open_Dispatches_and_Utilization_is_zero()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine.Id, 1);
            var truck = await CreateTruck();
            var driver = SetDefaultDriverForTruck(truck.Id);
            var orderLineTruckEntity = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);

            // Act, Assert
            await _schedulingAppService.SetOrderTruckUtilization(new OrderLineTruckUtilizationEditDto()
            {
                OrderLineTruckId = orderLineTruckEntity.Id,
                MaxUtilization = 1,
                Utilization = 0,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

    }
}
