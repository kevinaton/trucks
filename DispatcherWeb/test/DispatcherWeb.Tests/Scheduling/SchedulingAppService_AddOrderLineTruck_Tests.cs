using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_AddOrderLineTruck_Tests : AppTestBase, IAsyncLifetime
	{
		private ISchedulingAppService _schedulingAppService;
		private ISmsSender _smsSender;
		private int _officeId;

		public async Task InitializeAsync()
		{
			var office = await CreateOfficeAndAssignUserToIt();
			_officeId = office.Id;
			_smsSender = Substitute.For<ISmsSender>();
			var dispatchingAppService = Resolve<IDispatchingAppService>(new { smsSender = _smsSender });
			_schedulingAppService = Resolve<ISchedulingAppService>(new { smsSender = _smsSender, dispatchingAppService = dispatchingAppService });
			((DispatcherWebAppServiceBase)_schedulingAppService).Session = CreateSession();
		}

		[Fact]
		public async Task Test_AddOrderLineTruck_should_add_Trailer_when_it_added_to_another_OrderLine()
		{
			// Arrange
			DateTime date = Clock.Now.Date;
			var order = await CreateOrderWithOrderLines(date);
			var orderLine1 = order.OrderLines.ToArray()[0];
			var orderLine2 = order.OrderLines.ToArray()[1];
            var tractors = await CreateTractorVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
			var tractorTruck = await CreateTruck("301", tractors, _officeId, canPullTrailer: true);
			var trailerTruck = await CreateTruck("201", trailers, _officeId);
            var driver = await CreateDriver();
            await CreateOrderLineTruck(tractorTruck.Id, driver.Id, orderLine1.Id, .5m);
			await CreateOrderLineTruck(trailerTruck.Id, null, orderLine1.Id, 1);
			var parentOrderLineTruck = await CreateOrderLineTruck(tractorTruck.Id, driver.Id, orderLine2.Id, .5m);

			// Act
			var result = await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput
			{
				OrderLineId = orderLine2.Id,
				ParentId = parentOrderLineTruck.Id,
				TruckId = trailerTruck.Id,
			});

			// Assert
			result.IsFailed.ShouldBeFalse();
		}

		[Fact]
		public async Task Test_AddOrderLineTruck_should_throw_Exception_when_NotLeasedTruck_has_no_driver()
		{
			// Arrange
			DateTime date = Clock.Now.Date;
			var order = await CreateOrderWithOrderLines(date);
			var orderLine = order.OrderLines.ToArray()[0];
			var truck = await CreateTruck(alwaysShowOnSchedule: false);

			// Act, Assert
			await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput
			{
				OrderLineId = orderLine.Id,
				TruckId = truck.Id,
			}).ShouldThrowAsync(typeof(UserFriendlyException));
		}

		[Fact]
		public async Task Test_AddOrderLineTruck_should_add_LeaseTruck_with_no_driver()
		{
			// Arrange
			DateTime date = Clock.Now.Date;
			var order = await CreateOrderWithOrderLines(date);
			var orderLine = order.OrderLines.ToArray()[0];
			var truck = await CreateTruck(alwaysShowOnSchedule: true);

			// Act
			var result = await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput
			{
				OrderLineId = orderLine.Id,
				TruckId = truck.Id,
			});

			// Assert
			result.OrderUtilization.ShouldBe(1);
		}

        [Fact]
        public async Task Test_AddOrderLineTruck_should_throw_Exception_when_there_is_DriverAssignment_with_no_driver()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruck();
            await CreateDriverAssignmentForTruck(_officeId, truckEntity.Id, today);
            var order = await CreateOrderWithOrderLines(today);
            int orderLIneId = order.OrderLines.First().Id;

            // Act
            await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLIneId,
                TruckId = truckEntity.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_not_create_DriverAssignment_when_there_is_DriverAssignment_with_driver()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruck();
            var driver = await CreateDriver();
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truckEntity.Id, today, driver.Id);
            var order = await CreateOrderWithOrderLines(today);
            int orderLIneId = order.OrderLines.First().Id;

            // Act
            var result = await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLIneId,
                TruckId = truckEntity.Id,
            });

            // Assert
            result.IsFailed.ShouldBeFalse();
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            var da = driverAssignments[0];
            da.Id.ShouldBe(driverAssignment.Id);
        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_throw_UserFriendlyException_when_there_is_no_DefaultDriver()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            int orderLIneId = order.OrderLines.First().Id;

            // Act
            await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLIneId,
                TruckId = truckEntity.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_create_DriverAssignment_when_Truck_has_DefaultDriver()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruck();
            await SetDefaultDriverForTruck(truckEntity.Id);
            var order = await CreateOrderWithOrderLines(today);
            int orderLIneId = order.OrderLines.First().Id;

            // Act
            var result = await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLIneId,
                TruckId = truckEntity.Id,
            });

            // Assert
            result.IsFailed.ShouldBeFalse();
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            var da = driverAssignments[0];
            da.OfficeId.ShouldBe(_officeId);
            da.TruckId.ShouldBe(truckEntity.Id);
        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_create_DriverAssignment_for_Shift_when_there_is_NoDriver_for_another_shift()
        {
            // Assign
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruck();
            await SetDefaultDriverForTruck(truckEntity.Id);
            var shift2DriverAssignment = await CreateDriverAssignmentForTruck(_officeId, truckEntity.Id, today, Shift.Shift2);
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            int orderLIneId = order.OrderLines.First().Id;

            // Act
            var result = await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLIneId,
                TruckId = truckEntity.Id,
            });

            // Assert
            result.IsFailed.ShouldBeFalse();
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.Where(da => da.Id != shift2DriverAssignment.Id).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            var createdDriverAssignment = driverAssignments[0];
            createdDriverAssignment.OfficeId.ShouldBe(_officeId);
            createdDriverAssignment.TruckId.ShouldBe(truckEntity.Id);
            createdDriverAssignment.Shift.ShouldBe(Shift.Shift1);

        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_throw_UserFriendlyException_for_Order_from_another_office()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrderWithOrderLines(date);
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            var truck = await CreateTruck();
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;

            await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLineId,
                TruckId = truck.Id,
            })
                .ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_create_OrderLineTruck_for_Order_from_shared_office()
        {
            DateTime date = Clock.Now.Date;
            var orderEntity = await CreateOrderWithOrderLines(date);
            int orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FindAsync(orderLineId);
                orderLine.SharedOrderLines = new List<SharedOrderLine>()
                {
                    new SharedOrderLine { TenantId = 1, OfficeId = orderEntity.LocationId }
                };
            });
            var truck = await CreateTruck();
            await SetDefaultDriverForTruck(truck.Id);

            // Act
            var result = await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput()
            {
                OrderLineId = orderLineId,
                TruckId = truck.Id,
            });

            // Assert
            result.IsFailed.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == orderLineId).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_AddOrderLineTruck_should_create_DriverAssignment_when_adding_LeaseHauler_and_AllowLeaseHaulersFeature_is_true()
        {
            // Arrange
            ((ApplicationService)_schedulingAppService).SubstituteAllowLeaseHaulersFeature(true);
            DateTime date = DateTime.Today;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerDriver = await CreateLeaseHaulerDriver(leaseHauler.Id);
            var leaseHaulerTruck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            var availableLeaseHaulerTruck = await CreateAvailableLeaseHaulerTruck(leaseHauler.Id, leaseHaulerTruck.Id, leaseHaulerDriver.Id, date, Shift.Shift1, _officeId);

            var input = new AddOrderLineTruckInput()
            {
                OrderLineId = orderLine.Id,
                TruckId = leaseHaulerTruck.Id,
            };

            // Act
            var result = await _schedulingAppService.AddOrderLineTruck(input);

            // Assert
            result.IsFailed.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == orderLine.Id).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].DriverId.ShouldBe(leaseHaulerDriver.Id);
            driverAssignments[0].TruckId.ShouldBe(leaseHaulerTruck.Id);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Shift.ShouldBe(Shift.Shift1);
        }


        public Task DisposeAsync()
		{
			return Task.CompletedTask;
		}
	}
}
