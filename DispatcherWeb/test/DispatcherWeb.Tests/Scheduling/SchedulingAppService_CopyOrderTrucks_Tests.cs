using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Application.Services;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_CopyOrderTrucks_Tests : AppTestBase, IAsyncLifetime
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
        public async Task Test_CopyOrderTrucks_should_copy_single_OrderLine_with_trucks_and_DriverAssignments_from_original_to_new_Order()
        {
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);

            var orderLine2 = originalOrder.OrderLines.First(ol => ol.LineNumber == 2);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var driver = await CreateDriver();
            var truck1 = await CreateTruck("101");
            truck1 = await UpdateEntity(truck1, t => t.DefaultDriverId = driver.Id);
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 1);
            var truck2 = await CreateTruck("102");
            truck2 = await UpdateEntity(truck2, t => t.DefaultDriverId = driver.Id);
            await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine2.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, originalDate, driver.Id);

            DateTime newOrderDate = originalDate.AddDays(1);
            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate,
                DateEnd = newOrderDate,
                OrderId = originalOrder.Id,
                OrderLineId = orderLine2.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(1);
            int newOrderId = newOrderIds[0];

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrderId,
                OriginalOrderId = originalOrder.Id,
                OrderLineId = orderLine2.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLine.OrderId == newOrderId).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            var driverAssignments = await UsingDbContext(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(2);
            var newDriverAssignment = driverAssignments.First(da => da.Id != driverAssignment.Id);
            newDriverAssignment.Date.ShouldBe(originalDate.AddDays(1));
            newDriverAssignment.DriverId.ShouldBe(driver.Id);
            newDriverAssignment.TruckId.ShouldBe(truck2.Id);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_copy_single_OrderLine_with_LeaseHauler_trucks_from_original_to_new_Order()
        {
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 2);

            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var truck1 = await CreateTruck("401", alwaysShowOnSchedule: true);
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 1);
            var truck2 = await CreateTruck("501", await CreateTractorVehicleCategory(), canPullTrailer: true);
            await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine1.Id, 1);

            DateTime newOrderDate = originalDate.AddDays(1);
            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate,
                DateEnd = newOrderDate,
                OrderId = originalOrder.Id,
                OrderLineId = orderLine1.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(1);
            int newOrderId = newOrderIds[0];

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrderId,
                OriginalOrderId = originalOrder.Id,
                OrderLineId = orderLine1.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            result.SomeTrucksAreNotCopied.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLine.OrderId == newOrderId).ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);
            var driverAssignments = await UsingDbContext(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_not_copy_single_OrderLine_with_LeaseHauler_trucks_when_AllowLeaseHaulersFeature_is_true()
        {
            var featureChecker = Substitute.For<IFeatureChecker>();
            featureChecker.GetValueAsync(AppFeatures.AllowLeaseHaulersFeature).Returns("true");
            ((ApplicationService)_schedulingAppService).FeatureChecker = featureChecker;

            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 2);

            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var truck1 = await CreateTruck("401", alwaysShowOnSchedule: true);
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);
            var truck2 = await CreateTruck("501", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var driver2 = await CreateDriver();
            await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine1.Id, 1);

            DateTime newOrderDate = originalDate.AddDays(1);
            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate,
                DateEnd = newOrderDate,
                OrderId = originalOrder.Id,
                OrderLineId = orderLine1.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(1);
            int newOrderId = newOrderIds[0];

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrderId,
                OriginalOrderId = originalOrder.Id,
                OrderLineId = orderLine1.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            result.SomeTrucksAreNotCopied.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLine.OrderId == newOrderId).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_copy_OrderLine_with_trucks_but_dont_create_DriverAssignment_when_there_is_DriverAssignment_for_Date_Shift()
        {
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate, Shift.Shift1);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);

            var driver = await CreateDriver();
            var truck1 = await CreateTruck("101");
            truck1 = await UpdateEntity(truck1, t => t.DefaultDriverId = driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 0);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck1.Id, originalDate, Shift.Shift1, driver.Id);


            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            ((AbpServiceBase)orderAppService).SubstituteSetting(AppSettings.General.UseShifts, "true");
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = originalDate,
                DateEnd = originalDate,
                Shifts = new[] { Shift.Shift1 },
                OrderId = originalOrder.Id,
                OrderLineId = orderLine1.Id,
            });
            newOrderIds.Length.ShouldBe(1);
            int newOrderId = newOrderIds[0];

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrderId,
                OriginalOrderId = originalOrder.Id,
                OrderLineId = orderLine1.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLine.OrderId == newOrderId).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            var driverAssignments = await UsingDbContext(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            var newDriverAssignment = driverAssignments.First();
            newDriverAssignment.Date.ShouldBe(originalDate);
            newDriverAssignment.DriverId.ShouldBe(driver.Id);
            newDriverAssignment.TruckId.ShouldBe(truck1.Id);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_not_copy_truck_when_truck_is_fully_utilized_on_Date_and_Shift()
        {
            // Arrange
            DateTime originalDate = DateTime.Today;
            Shift originalShift = Shift.Shift1;
            var order1 = await CreateOrderWithOrderLines(originalDate);
            order1 = await UpdateEntity(order1, o => o.Shift = originalShift);
            var orderLine1 = order1.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var truck1 = await CreateTruck("101");
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);

            DateTime destinationDate = originalDate;
            Shift destinationShift = Shift.Shift2;
            var order2 = await CreateOrderWithOrderLines(originalDate);
            order2 = await UpdateEntity(order2, o => o.Shift = destinationShift);
            var orderLine2 = order2.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine2.Id, 1);
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine2.Id, 1);

            // Copy order1 to newOrder
            var newOrder = await CreateOrderWithOrderLines(destinationDate);
            newOrder = await UpdateEntity(newOrder, o => o.Shift = destinationShift);
            await SetOrderLineNumberOfTrucks(newOrder.OrderLines.First().Id, 1);

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrder.Id,
                OriginalOrderId = order1.Id,
            });

            // Assert
            result.Completed.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_not_copy_truck_when_there_is_no_DriverAssignment_on_date()
        {
            // Arrange
            DateTime originalDate = DateTime.Today;
            var order1 = await CreateOrderWithOrderLines(originalDate);
            var orderLine1 = order1.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var truck1 = await CreateTruck("101");
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);

            DateTime destinationDate = originalDate.AddDays(1);
            await CreateDriverAssignmentForTruck(_officeId, truck1.Id, destinationDate, null);

            // Copy order1 to newOrder
            var newOrder = await CreateOrderWithOrderLines(destinationDate);
            await SetOrderLineNumberOfTrucks(newOrder.OrderLines.First().Id, 1);

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrder.Id,
                OriginalOrderId = order1.Id,
            });

            // Assert
            result.SomeTrucksAreNotCopied.ShouldBeTrue();
            var trucks = await UsingDbContextAsync(async context => await context.Trucks.Where(t => t.Id != truck1.Id).ToListAsync());
            trucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_not_copy_truck_when_there_is_no_DriverAssignment_on_date_and_shift()
        {
            // Arrange
            DateTime originalDate = DateTime.Today;
            var order1 = await CreateOrderWithOrderLines(originalDate, Shift.Shift1);
            var orderLine1 = order1.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var truck1 = await CreateTruck("101");
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);

            DateTime destinationDate = originalDate.AddDays(1);

            // Copy order1 to newOrder
            var newOrder = await CreateOrderWithOrderLines(destinationDate);
            await SetOrderLineNumberOfTrucks(newOrder.OrderLines.First().Id, 1);

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrder.Id,
                OriginalOrderId = order1.Id,
            });

            // Assert
            result.SomeTrucksAreNotCopied.ShouldBeTrue();
            var trucks = await UsingDbContextAsync(async context => await context.Trucks.Where(t => t.Id != truck1.Id).ToListAsync());
            trucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_copy_trucks_from_original_to_new_Order()
        {
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 2);

            var orderLine2 = originalOrder.OrderLines.First(ol => ol.LineNumber == 2);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var driver = await CreateDriver();
            var truck1 = await CreateTruck("101");
            truck1 = await UpdateEntity(truck1, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck1.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 1);
            var truck12 = await CreateTruck("102");
            truck12 = await UpdateEntity(truck12, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck12.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck12.Id, driver.Id, orderLine1.Id, 1);
            var truck2 = await CreateTruck("103");
            truck2 = await UpdateEntity(truck2, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck2.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine2.Id, 1);

            DateTime newOrderDate = originalDate.AddDays(1);
            var newOrder = await CreateOrderWithOrderLines(newOrderDate);
            foreach (var newOrderOrderLine in newOrder.OrderLines)
            {
                await SetOrderLineNumberOfTrucks(newOrderOrderLine.Id, 2);
            }

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrder.Id,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
            {
                return await context.OrderLineTrucks
                    .Where(olt => olt.OrderLine.OrderId == newOrder.Id)
                    .ToListAsync();
            });
            orderLineTrucks.Count.ShouldBe(3);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_copy_IsDone_trucks_from_original_to_new_Order_and_set_correct_Utilization()
        {
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 2);

            var orderLine2 = originalOrder.OrderLines.First(ol => ol.LineNumber == 2);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var driver = await CreateDriver();
            var truck1 = await CreateTruck("101");
            truck1 = await UpdateEntity(truck1, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck1.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 0);
            var truck12 = await CreateTruck("102");
            truck12 = await UpdateEntity(truck12, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck12.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck12.Id, driver.Id, orderLine1.Id, 0);
            var truck2 = await CreateTruck("103");
            truck2 = await UpdateEntity(truck2, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck2.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine2.Id, 0);

            DateTime newOrderDate = originalDate.AddDays(1);
            var newOrder = await CreateOrderWithOrderLines(newOrderDate);
            foreach (var newOrderOrderLine in newOrder.OrderLines)
            {
                await SetOrderLineNumberOfTrucks(newOrderOrderLine.Id, 2);
            }

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrder.Id,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
            {
                return await context.OrderLineTrucks
                    .Where(olt => olt.OrderLine.OrderId == newOrder.Id)
                    .ToListAsync();
            });
            orderLineTrucks.Count.ShouldBe(3);
            orderLineTrucks[0].IsDone.ShouldBeFalse();
            orderLineTrucks[0].Utilization.ShouldBe(1);
            orderLineTrucks[1].IsDone.ShouldBeFalse();
            orderLineTrucks[1].Utilization.ShouldBe(1);
            orderLineTrucks[2].IsDone.ShouldBeFalse();
            orderLineTrucks[2].Utilization.ShouldBe(1);
        }

        [Fact]
        public async Task Test_CopyOrderTrucks_should_copy_IsDone_trucks_from_original_to_new_Order_and_distribute_Utilization()
        {
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 2);

            var orderLine2 = originalOrder.OrderLines.First(ol => ol.LineNumber == 2);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 1);
            var driver = await CreateDriver();
            var truck1 = await CreateTruck("101");
            truck1 = await UpdateEntity(truck1, t => t.DefaultDriverId = driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck1.Id, originalDate, driver.Id);
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 0);
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 0);
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine2.Id, 0);

            DateTime newOrderDate = originalDate.AddDays(1);
            var newOrder = await CreateOrderWithOrderLines(newOrderDate);
            foreach (var newOrderOrderLine in newOrder.OrderLines)
            {
                await SetOrderLineNumberOfTrucks(newOrderOrderLine.Id, 2);
            }

            // Act
            var result = await _schedulingAppService.CopyOrderTrucks(new CopyOrderTrucksInput()
            {
                NewOrderId = newOrder.Id,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
            {
                return await context.OrderLineTrucks
                    .OrderBy(olt => olt.Id)
                    .Where(olt => olt.OrderLine.OrderId == newOrder.Id)
                    .ToListAsync();
            });
            orderLineTrucks.Count.ShouldBe(3);
            orderLineTrucks[0].IsDone.ShouldBeFalse();
            orderLineTrucks[0].Utilization.ShouldBe(0.33m);
            orderLineTrucks[1].IsDone.ShouldBeFalse();
            orderLineTrucks[1].Utilization.ShouldBe(0.34m);
            orderLineTrucks[2].IsDone.ShouldBeFalse();
            orderLineTrucks[2].Utilization.ShouldBe(0.33m);
        }



        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
