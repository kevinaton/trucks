using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_SetOrderLineScheduledTrucks_Tests : AppTestBase, IAsyncLifetime
	{
		private int _officeId;
		private ISchedulingAppService _schedulingAppService;

		public async Task InitializeAsync()
		{
			var office = await CreateOfficeAndAssignUserToIt();
			_officeId = office.Id;
			_schedulingAppService = Resolve<ISchedulingAppService>();
			((DispatcherWebAppServiceBase)_schedulingAppService).Session = CreateSession();
		}

		[Fact]
		public async Task Test_SetOrderLineScheduledTrucks_should_update_ScheduledTrucks()
		{
			// Arrange
			DateTime date = Clock.Now.Date;
			var order = await CreateOrderWithOrderLines(date);
			var orderLine = order.OrderLines.First();

			// Act
			var result = await _schedulingAppService.SetOrderLineScheduledTrucks(new SetOrderLineScheduledTrucksInput()
            {
                OrderLineId = orderLine.Id,
                ScheduledTrucks = 3,
            });

			// Assert
			result.ScheduledTrucks.ShouldBe(3);
			result.OrderUtilization.ShouldBe(0);
			var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(orderLine.Id));
			updatedOrderLine.ScheduledTrucks.ShouldBe(3);
		}

		[Fact]
		public async Task Test_SetOrderLineScheduledTrucks_should_update_ScheduledTrucks_when_OrderLineTruck_exists()
		{
			// Arrange
			DateTime date = Clock.Now.Date;
			var order = await CreateOrderWithOrderLines(date);
			var orderLine = order.OrderLines.First();
			var truck = await CreateTruck();
			var driver = await CreateDriver();
			var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

			// Act
			var result = await _schedulingAppService.SetOrderLineScheduledTrucks(new SetOrderLineScheduledTrucksInput()
            {
                OrderLineId = orderLine.Id,
                ScheduledTrucks = 1,
            });

			// Assert
			result.ScheduledTrucks.ShouldBe(1);
			result.OrderUtilization.ShouldBe(1);
			var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(orderLine.Id));
			updatedOrderLine.ScheduledTrucks.ShouldBe(1);
		}

		[Fact]
		public async Task Test_SetOrderLineScheduledTrucks_should_throw_UserFriendlyException_when_ScheduledTrucks_is_less_than_Utilization()
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

			// Act, Assert
			var result = await _schedulingAppService.SetOrderLineScheduledTrucks(new SetOrderLineScheduledTrucksInput()
            {
                OrderLineId = orderLine.Id,
                ScheduledTrucks = 1,
            }).ShouldThrowAsync(typeof(ApplicationException));
		}

		public Task DisposeAsync()
		{
			return Task.CompletedTask;
		}
	}
}
