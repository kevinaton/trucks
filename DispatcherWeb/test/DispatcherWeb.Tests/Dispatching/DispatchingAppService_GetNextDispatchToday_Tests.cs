using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_GetNextDispatchToday_Tests : DispatchingAppService_Tests_Base, IAsyncLifetime
    {
        public new async Task InitializeAsync()
        {
            await base.InitializeAsync();
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(DispatchVia.SimplifiedSms.ToIntString());
        }

        [Theory]
        [InlineData(DispatchStatus.Completed, DispatchStatus.Created)]
        [InlineData(DispatchStatus.Completed, DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Completed, DispatchStatus.Loaded)]
        [InlineData(DispatchStatus.Canceled, DispatchStatus.Created)]
        [InlineData(DispatchStatus.Canceled, DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Canceled, DispatchStatus.Loaded)]
        public async Task Test_GetNextDispatchToday_should_return_Next_Dispatch_when_current_Dispatch_is_complete_or_canceled_and_there_is_next_dispatch(
            DispatchStatus currentDispatchStatus, DispatchStatus nextDispatchStatus)
        {
            Debug.Assert(currentDispatchStatus == DispatchStatus.Canceled || currentDispatchStatus == DispatchStatus.Completed);
            Debug.Assert(nextDispatchStatus == DispatchStatus.Created || nextDispatchStatus == DispatchStatus.Acknowledged || nextDispatchStatus == DispatchStatus.Loaded);
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, currentDispatchStatus);
                await UpdateEntity(dispatch, d =>
                {
                    d.Sent = today;
                    if (currentDispatchStatus == DispatchStatus.Completed)
                    {
                        d.Acknowledged = today;
                    }
                });
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, nextDispatchStatus);

            // Act 
            GetNextDispatchTodayResult result;
            using (UsingTenantId(null))
            {
                result = await _dispatchingAppService.GetNextDispatchToday(dispatch.Guid);
            }

            // Assert
            result.DispatchExists.ShouldBeTrue();
            result.DispatchGuid.ShouldBe(dispatch2.Guid);
        }

        [Theory]
        [InlineData(DispatchStatus.Created, DispatchStatus.Created)]
        [InlineData(DispatchStatus.Created, DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Created, DispatchStatus.Loaded)]
        [InlineData(DispatchStatus.Sent, DispatchStatus.Created)]
        [InlineData(DispatchStatus.Sent, DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Sent, DispatchStatus.Loaded)]
        public async Task Test_GetNextDispatchToday_should_return_Next_Dispatch_when_current_Dispatch_is_deleted_and_status_is_created_or_sent_and_there_is_next_dispatch(
            DispatchStatus currentDispatchStatus, DispatchStatus nextDispatchStatus)
        {
            Debug.Assert(currentDispatchStatus == DispatchStatus.Created || currentDispatchStatus == DispatchStatus.Sent);
            Debug.Assert(nextDispatchStatus == DispatchStatus.Created || nextDispatchStatus == DispatchStatus.Acknowledged || nextDispatchStatus == DispatchStatus.Loaded);
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, currentDispatchStatus);
                await UpdateEntity(dispatch, d =>
                {
                    d.IsDeleted = true;
                    d.Sent = today;
                    if (currentDispatchStatus == DispatchStatus.Completed)
                    {
                        d.Acknowledged = today;
                    }
                });
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, nextDispatchStatus);

            // Act 
            GetNextDispatchTodayResult result;
            using (UsingTenantId(null))
            {
                result = await _dispatchingAppService.GetNextDispatchToday(dispatch.Guid);
            }

            // Assert
            result.DispatchExists.ShouldBeTrue();
            result.DispatchGuid.ShouldBe(dispatch2.Guid);
        }

        [Fact]
        public async Task Test_GetNextDispatchToday_should_return_DispatchExists_false_when_current_Dispatch_is_complete_and_there_is_no_next_dispatch()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Completed);
            await UpdateEntity(dispatch, d =>
            {
                d.Sent = today;
                d.Acknowledged = today;
            });

            // Act 
            GetNextDispatchTodayResult result;
            using (UsingTenantId(null))
            {
                result = await _dispatchingAppService.GetNextDispatchToday(dispatch.Guid);
            }

            // Assert
            result.DispatchExists.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_GetNextDispatchToday_should_throw_ArgumentException_when_there_is_no_dispatch_with_Guid()
        {
            // Arrange

            // Act, Assert
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.GetNextDispatchToday(Guid.NewGuid()).ShouldThrowAsync(typeof(ArgumentException));
            }
        }

        [Theory]
        [InlineData(DispatchStatus.Created)]
        [InlineData(DispatchStatus.Sent)]
        [InlineData(DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Loaded)]
        [InlineData(DispatchStatus.Error)]
        public async Task Test_GetNextDispatchToday_should_throw_ArgumentException_when_dispatch_with_Guid_is_not_Completed_or_Canceled(DispatchStatus dispatchStatus)
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, dispatchStatus);

            // Act, Assert
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.GetNextDispatchToday(dispatch.Guid).ShouldThrowAsync(typeof(ArgumentException));
            }
        }

        [Theory]
        [InlineData(DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Loaded)]
        [InlineData(DispatchStatus.Error)]
        [InlineData(DispatchStatus.Completed)]
        [InlineData(DispatchStatus.Canceled)]
        public async Task Test_GetNextDispatchToday_should_throw_ArgumentException_when_dispatch_with_Guid_is_deleted_and_is_not_Created_or_Sent(DispatchStatus dispatchStatus)
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, dispatchStatus);
            await UpdateEntity(dispatch, d =>
            {
                d.IsDeleted = true;
                d.Sent = today;
            });

            // Act, Assert
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.GetNextDispatchToday(dispatch.Guid).ShouldThrowAsync(typeof(ArgumentException))
                    ;
            }
        }

    }
}
