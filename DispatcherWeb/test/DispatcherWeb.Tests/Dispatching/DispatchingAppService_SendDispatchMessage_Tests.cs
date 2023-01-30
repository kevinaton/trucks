using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Collections.Extensions;
using Abp.Notifications;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_SendDispatchMessage_Tests : DispatchingAppService_Tests_Base
    {
        [Theory]
        [InlineData(DispatchVia.SimplifiedSms)]
        public async Task Test_SendDispatchMessage_should_return_false_and_send_error_notification_when_driver_does_not_have_phone(DispatchVia dispatchVia)
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };
            await UsingDbContextAsync(async context =>
            {
                var driver = await context.Drivers.FindAsync(truckEntity.DefaultDriverId);
                driver.CellPhoneNumber = null;
            });
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(dispatchVia.ToIntString());

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_dispatch_and_do_not_send_error_notification_when_driver_does_not_have_phone_and_DispatchViaSms_is_false()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };
            await UsingDbContextAsync(async context =>
            {
                var driver = await context.Drivers.FindAsync(truckEntity.DefaultDriverId);
                driver.CellPhoneNumber = null;
            });
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(DispatchVia.None.ToIntString());

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            //result.ShouldBeTrue();
            await _appNotifier.DidNotReceive().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.ToListAsync());
            dispatches.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_dispatch_and_send_sms_to_driver()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "message",
                NumberOfDispatches = 1,
                IsMultipleLoads = true,
            };
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync("", "", true);
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.ToListAsync());
            dispatches.Count.ShouldBe(1);
            var dispatch = dispatches.First();
            dispatch.Status.ShouldBe(DispatchStatus.Sent);
            dispatch.Sent.ShouldNotBeNull();
            dispatch.IsMultipleLoads.ShouldBeTrue();
            dispatch.Message.ShouldContain("Click this link to acknowledge this dispatch");
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_dispatch_but_not_send_sms_to_driver_when_DispatchViaSms_is_false()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "message",
                NumberOfDispatches = 1,
            };
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(DispatchVia.None.ToIntString());
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync("", "", true);
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.ToListAsync());
            dispatches.Count.ShouldBe(1);
            var dispatch = dispatches.First();
            dispatch.Status.ShouldBe(DispatchStatus.Created);
            dispatch.Sent.ShouldBeNull();
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_start_message_with_Multiple_loads_when_MultipleLoads_is_checked()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "message",
                NumberOfDispatches = 1,
                IsMultipleLoads = true,
            };
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync("", "", true);
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.ToListAsync());
            dispatches.Count.ShouldBe(1);
            var dispatch = dispatches.First();
            dispatch.Message.ShouldStartWith("Run Until Stopped. ");
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_not_contain_Pickup_and_Delivery_notes() // #8530
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "message",
                NumberOfDispatches = 1,
            };
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync("", "", true);
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.ToListAsync());
            dispatches.Count.ShouldBe(1);
            var dispatch = dispatches.First();
            dispatch.Message.ShouldNotContain("Load Instructions: note");
            dispatch.Message.ShouldNotContain("Destination Instructions: note");
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_2_dispatch_and_send_1_sms_to_driver()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 2,
            };
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync("", "", true);
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.ToListAsync());
            dispatches.Count.ShouldBe(2);
            dispatches.Count(d => d.Status == DispatchStatus.Created).ShouldBe(1);
            dispatches.First(d => d.Status == DispatchStatus.Created).Sent.ShouldBeNull();
            dispatches.Count(d => d.Status == DispatchStatus.Sent).ShouldBe(1);
            dispatches.First(d => d.Status == DispatchStatus.Sent).Sent.ShouldNotBeNull();
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_second_dispatch_and_dont_send_sms_to_driver()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 2,
            };
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act, First dispatch
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync("", "", true);

            _smsSender.ClearReceivedCalls();

            // Act, Second dispatch
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync("", "", true);
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_return_false_and_send_error_notification_when_there_was_an_error()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Failed, 1, "Test error"));

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            var dispatch = await UsingDbContextAsync(async context => await context.Dispatches.FirstAsync());
            dispatch.Status.ShouldBe(DispatchStatus.Error);
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_return_false_and_send_MessageFailed_notification_when_message_length_exceeds_500()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = Enumerable.Repeat("test1", 101).JoinAsString(""),
                NumberOfDispatches = 1,
            };

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            var dispatchCount = await UsingDbContextAsync(async context => await context.Dispatches.CountAsync());
            dispatchCount.ShouldBe(0);
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_throw_ArgumentException_when_order_is_in_past()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(-1);
            var truckEntity = await CreateTruckEntity(true);
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };

            // Act, Assert
            await _dispatchingAppService.SendDispatchMessage(input).ShouldThrowAsync(typeof(ArgumentException));
        }

        [Fact(Skip = "#8456 (The dispatcher should be able to initiate a dispatch to the driver without a start time scheduled)")]
        public async Task Test_SendDispatchMessage_should_throw_UserFriendlyException_when_there_is_no_DriverAssignment_for_TruckDriver()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };

            // Act, Assert
            await _dispatchingAppService.SendDispatchMessage(input).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_not_throw_UserFriendlyException_when_there_is_DriverAssignment_with_null_StartTime_for_TruckDriver()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date);
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            _smsSender.SendAsync("", "", true).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, 1, "Test"));
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            // There is no exception, the test is fine.
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_Dispatch_and_send_error_notification_when_driver_is_not_User_and_DispatchVia_is_DriverApplication()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(DispatchVia.DriverApplication.ToIntString());

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Warn);
        }

        [Fact]
        public async Task Test_SendDispatchMessage_should_create_Dispatch_and_do_not_send_error_notification_when_driver_not_User_and_DispatchVia_is_None()
        {
            // Arrange
            DateTime date = DateTime.Today;
            var truckEntity = await CreateTruckEntity(true);
            await CreateDriverAssignment(truckEntity, date, date.AddHours(10));
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity);
            var orderLine = orderEntity.OrderLines.First(ol => ol.OrderLineTrucks.Any());
            var orderLineTruckId = orderLine.OrderLineTrucks.First(x => x.TruckId == truckEntity.Id).Id;
            SendDispatchMessageInput input = new SendDispatchMessageInput()
            {
                OrderLineId = orderLine.Id,
                OrderLineTruckIds = new[] { orderLineTruckId },
                Message = "test",
                NumberOfDispatches = 1,
            };
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(DispatchVia.None.ToIntString());

            // Act
            await _dispatchingAppService.SendDispatchMessage(input);

            // Assert
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
        }



        private async Task<DriverAssignment> CreateDriverAssignment(Truck truck, DateTime date, DateTime? startTime = null)
        {
            var driverAssignment = await CreateDriverAssignmentForTruck(truck.LocationId, truck.Id, date, null, truck.DefaultDriverId);
            await UpdateEntity(driverAssignment, da => da.StartTime = startTime);
            return driverAssignment;
        }


    }
}
