using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Services;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class ShedulingAppService_Tests : SchedulingAppService_Tests_Base
    {


        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_is_true_when_there_is_Created_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Created);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeTrue();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }
        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_is_true_when_there_is_Sent_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Sent);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeTrue();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }
        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_AcknowledgedOrLoaded_is_true_when_there_is_Acknowledged_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Acknowledged);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeTrue();
        }
        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_AcknowledgedOrLoaded_is_true_when_there_is_Loaded_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Loaded);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeTrue();
        }
        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_and_AcknowledgedOrLoaded_are_false_when_there_is_Completed_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Completed);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }
        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_and_AcknowledgedOrLoaded_are_false_when_there_is_Canceled_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Canceled);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }
        private async Task<DeleteOrderLineTruckInput> CreateHasDispatchesTestData(DispatchStatus status)
        {
            DateTime today = Clock.Now.Date;
            var orderEntity = await CreateOrder(today);
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLineId, status);
            return new DeleteOrderLineTruckInput()
            {
                OrderLineTruckId = orderLineTruck.Id,
            };
        }

        [Fact]
        public async Task Test_SetOrderLineLoads_should_throw_UserFriendlyException_for_Order_from_another_office()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;

            await _schedulingAppService.SetOrderLineLoads(new SetOrderLineLoadsInput()
            {
                    OrderLineId = orderLineId,
                    Loads = 1,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_GetOrderTruckUtilizationForEdit_should_return_utilization_for_one_day()
        {
            DateTime date1 = DateTime.Today;
            var orderEntity = await CreateOrder(date1);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine.Id, 1);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruckEntity = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 0.5m);

            DateTime date2 = date1.AddDays(-1);
            var orderEntity2 = await CreateOrder(date2);
            orderEntity2.OrderLines.Count.ShouldBe(2);
            var orderLine2 = orderEntity2.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine2.Id, 1);
            var orderLineTruckEntity2 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 1);

            var oltUtilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruckEntity.Id));

            oltUtilization.Utilization.ShouldBe(0.5m);
            oltUtilization.MaxUtilization.ShouldBe(1);
            oltUtilization.OrderLineTruckId.ShouldBe(orderLineTruckEntity.Id);
        }

        [Fact]
        public async Task Test_GetOrderTruckUtilizationForEdit_should_return_utilization_for_partially_allocated_Truck()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine.Id, 1);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruckEntity = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 0.3m);

            var oltUtilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruckEntity.Id));

            oltUtilization.Utilization.ShouldBe(0.3m);
            oltUtilization.MaxUtilization.ShouldBe(1);
            oltUtilization.OrderLineTruckId.ShouldBe(orderLineTruckEntity.Id);
        }

        [Fact]
        public async Task Test_GetOrderTruckUtilizationForEdit_should_return_utilization_for_partially_allocated_Truck_on_different_Shifts()
        {
            DateTime date = Clock.Now.Date;
            var order1 = await CreateOrder(date);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            order1.OrderLines.Count.ShouldBe(2);
            var orderLine1 = order1.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);

            var order2 = await CreateOrder(date);
            order2 = await UpdateEntity(order2, o => o.Shift = Shift.Shift2);
            order2.OrderLines.Count.ShouldBe(2);
            var orderLine2 = order2.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine2.Id, 1);

            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck1 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 0.5m);
            var orderLineTruck2 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 0.5m);

            // Act
            var olt1Utilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruck1.Id));
            var olt2Utilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruck2.Id));

            // Assert
            olt1Utilization.Utilization.ShouldBe(0.5m);
            olt1Utilization.MaxUtilization.ShouldBe(1);
            olt1Utilization.OrderLineTruckId.ShouldBe(orderLineTruck1.Id);

            olt2Utilization.Utilization.ShouldBe(0.5m);
            olt2Utilization.MaxUtilization.ShouldBe(1);
            olt2Utilization.OrderLineTruckId.ShouldBe(orderLineTruck2.Id);
        }

        [Fact]
        public async Task Test_GetOrderTruckUtilizationForEdit_should_return_utilization_for_Truck_allocated_on_several_OrderLine()
        {
            DateTime date = DateTime.Today; 
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);

            var orderLine1 = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);

            var orderLine2 = orderEntity.OrderLines.Last();
            await SetOrderLineNumberOfTrucks(orderLine2.Id, 1);

            var truck = await CreateTruck();
            var driver = await CreateDriver();

            var orderLineTruckEntity1 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 0.3m);
            var orderLineTruckEntity2 = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 0.4m);

            var oltUtilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruckEntity1.Id));

            oltUtilization.Utilization.ShouldBe(0.3m);
            oltUtilization.MaxUtilization.ShouldBe(0.6m);
            oltUtilization.OrderLineTruckId.ShouldBe(orderLineTruckEntity1.Id);

            oltUtilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruckEntity2.Id));

            oltUtilization.Utilization.ShouldBe(0.4m);
            oltUtilization.MaxUtilization.ShouldBe(0.7m);
            oltUtilization.OrderLineTruckId.ShouldBe(orderLineTruckEntity2.Id);
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_set_IsComplete_and_delete_OrderLineTrucks()
        {
            DateTime originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);

            // Act
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput()
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLineId == orderLine.Id && !olt.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks[0].IsDone.ShouldBeTrue();
            orderLineTrucks[0].Utilization.ShouldBe(0);
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.Where(ol => ol.Id == orderLine.Id && !ol.IsDeleted).FirstAsync());
            updatedOrderLine.IsComplete.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_throw_ApplicationExcepiton_when_there_is_sent_dispatch()
        {
            DateTime originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);

            // Act, Assert
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput()
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_throw_ApplicationExcepiton_when_there_is_acknowledged_dispatch()
        {
            DateTime originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act, Assert
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput()
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_throw_ApplicationExcepiton_when_there_is_loaded_dispatch()
        {
            DateTime originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act, Assert
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput()
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_not_cancel_completed_dispatch()
        {
            DateTime originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Completed);

            // Act
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput()
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            });

            // Assert
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatchEntity.Id));
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
        }

        [Fact]
        public async Task Test_GetOrderTruckUtilizationForEdit_should_return_utilization_for_fully_allocated_Truck()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            await SetOrderLineNumberOfTrucks(orderLine.Id, 1);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruckEntity = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var oltUtilization = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(orderLineTruckEntity.Id));

            oltUtilization.Utilization.ShouldBe(1);
            oltUtilization.MaxUtilization.ShouldBe(1);
            oltUtilization.OrderLineTruckId.ShouldBe(orderLineTruckEntity.Id);
        }


        private async Task<Order> GetOrder(int orderId)
        {
            return await UsingDbContextAsync(async context => await
                context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderId));
        }

    }
}
