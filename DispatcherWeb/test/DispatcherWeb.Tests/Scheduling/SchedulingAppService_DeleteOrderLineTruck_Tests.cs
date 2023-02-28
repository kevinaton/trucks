using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_DeleteOrderLineTruck_Tests : SchedulingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_throw_UserFriendlyException_for_Order_from_another_office()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;

            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineId = orderLineId,
                OrderLineTruckId = 1,
            })
                .ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_throw_UserFriendlyException_for_shared_Order_from_another_office_and_not_their_Truck()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            int office1Id = orderEntity.LocationId;
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            var truck = await CreateTruck(officeId: office2.Id);
            var driver = await CreateDriver();
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FirstAsync(ol => ol.Id == orderLineId);
                orderLine.SharedOrderLines = new List<SharedOrderLine>()
                {
                    new SharedOrderLine() { TenantId = 1, OfficeId = office1Id }
                };
            });

            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineId = orderLineId,
                OrderLineTruckId = orderLineTruck.Id,
            })
                .ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_throw_ApplicationException_there_is_acknowledge_Dispatch()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLineId, DispatchStatus.Acknowledged);

            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineId = orderLineId,
                OrderLineTruckId = orderLineTruck.Id,
            })
                .ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_throw_ApplicationException_there_is_loaded_Dispatch()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLineId, DispatchStatus.Loaded);

            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineId = orderLineId,
                OrderLineTruckId = orderLineTruck.Id,
            })
                .ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_deleteOrderLineTruck_for_shared_Order_from_another_office_and_shared_Truck()
        {
            DateTime today = Clock.Now.Date;
            var orderEntity = await CreateOrder(today);
            int office1Id = orderEntity.LocationId;
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            var truck = await CreateTruck(officeId: office2.Id);
            var driver = await CreateDriver();
            await ShareTruck(truck.Id, office1Id, today);

            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FirstAsync(ol => ol.Id == orderLineId);
                orderLine.SharedOrderLines = new List<SharedOrderLine>()
                {
                    new SharedOrderLine() { TenantId = 1, OfficeId = office1Id }
                };
            });

            // Act
            var result = await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineId = orderLineId,
                OrderLineTruckId = orderLineTruck.Id,
            });

            // Assert
            result.OrderLineUtilization.ShouldBe(0m);
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_delete_open_dispatches()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var orderEntity = await CreateOrder(today);
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLineId, DispatchStatus.Sent);

            // Act
            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput()
            {
                OrderLineId = orderLineId,
                OrderLineTruckId = orderLineTruck.Id,
            });

            // Assert
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_set_IsDone_true_when_MarkAsDone_is_true()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var orderEntity = await CreateOrder(today);
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);

            // Act
            var result = await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput()
            {
                MarkAsDone = true,
                OrderLineId = orderLineId,
                OrderLineTruckId = orderLineTruck.Id,
            });

            // Assert
            result.OrderLineUtilization.ShouldBe(0);
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDeleted.ShouldBeFalse();
            updatedOrderLineTruck.IsDone.ShouldBeTrue();
            updatedOrderLineTruck.Utilization.ShouldBe(0);
        }

    }
}
