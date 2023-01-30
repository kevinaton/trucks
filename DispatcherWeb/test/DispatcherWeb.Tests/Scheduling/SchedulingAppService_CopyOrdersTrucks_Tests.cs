using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_CopyOrdersTrucks_Tests : AppTestBase, IAsyncLifetime
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
        public async Task Test_CopyOrdersTrucks_should_copy_trucks_for_2_Orders()
        {
            // Arrange
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

            DateTime newOrderDate = originalDate.AddDays(1);
            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate,
                DateEnd = newOrderDate.AddDays(1),
                OrderId = originalOrder.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(2);

            // Act
            var result = await _schedulingAppService.CopyOrdersTrucks(new CopyOrdersTrucksInput()
            {
                NewOrderIds = newOrderIds,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => newOrderIds.Contains(olt.OrderLine.OrderId)).ToListAsync());
            orderLineTrucks.Count.ShouldBe(4);
        }

        [Fact]
        public async Task Test_CopyOrdersTrucks_should_copy_Tractor_and_Trailer_trucks()
        {
            // Arrange
            DateTime originalDate = DateTime.Today;
            var originalOrder = await CreateOrderWithOrderLines(originalDate);

            originalOrder.OrderLines.Count.ShouldBe(2);
            var orderLine1 = originalOrder.OrderLines.First(ol => ol.LineNumber == 1);
            await SetOrderLineNumberOfTrucks(orderLine1.Id, 2);

            var driver = await CreateDriver();
            var truck1 = await CreateTruck("201", await CreateTrailerVehicleCategory());
            await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 1);
            var truck2 = await CreateTruck("302", await CreateTractorVehicleCategory(), canPullTrailer: true);
            truck2 = await UpdateEntity(truck2, t => t.DefaultDriverId = driver.Id);
            await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine1.Id, 1);

            DateTime newOrderDate = originalDate.AddDays(1);
            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate,
                DateEnd = newOrderDate,
                OrderId = originalOrder.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(1);

            // Act
            var result = await _schedulingAppService.CopyOrdersTrucks(new CopyOrdersTrucksInput()
            {
                NewOrderIds = newOrderIds,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            result.SomeTrucksAreNotCopied.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => newOrderIds.Contains(olt.OrderLine.OrderId)).ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Test_CopyOrdersTrucks_should_not_copy_trucks_for_2_Orders_when_Trucks_are_in_use()
        {
            // Arrange
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

            DateTime newOrderDate1 = originalDate.AddDays(1);
            DateTime newOrderDate2 = originalDate.AddDays(2);
            var newOrder1 = await CreateOrderWithOrderLines(newOrderDate1);
            var newOrder2 = await CreateOrderWithOrderLines(newOrderDate2);
            var newOrder1Line1 = newOrder1.OrderLines.First(ol => ol.LineNumber == 1);
            var newOrder2Line1 = newOrder2.OrderLines.First(ol => ol.LineNumber == 1);
            await CreateOrderLineTruck(truck1.Id, driver.Id, newOrder1Line1.Id, 1);
            await CreateOrderLineTruck(truck2.Id, driver.Id, newOrder2Line1.Id, 1);

            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate1,
                DateEnd = newOrderDate2,
                OrderId = originalOrder.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(2);

            // Act
            var result = await _schedulingAppService.CopyOrdersTrucks(new CopyOrdersTrucksInput()
            {
                NewOrderIds = newOrderIds,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.ConflictingTrucks.ShouldNotBeNull();
            result.ConflictingTrucks.Count.ShouldBe(2);
            result.ConflictingTrucks.ShouldContain(truck1.TruckCode);
            result.ConflictingTrucks.ShouldContain(truck2.TruckCode);
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => newOrderIds.Contains(olt.OrderLine.OrderId)).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_CopyOrdersTrucks_should_not_copy_trucks_for_all_Orders_when_Truck_for_1_Order_is_in_use()
        {
            // Arrange
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

            DateTime newOrderDate1 = originalDate.AddDays(1);
            DateTime newOrderDate2 = originalDate.AddDays(2);
            var newOrder1 = await CreateOrderWithOrderLines(newOrderDate1);
            var newOrder2 = await CreateOrderWithOrderLines(newOrderDate2);
            var newOrder1Line1 = newOrder1.OrderLines.First(ol => ol.LineNumber == 1);
            var newOrder2Line1 = newOrder2.OrderLines.First(ol => ol.LineNumber == 1);
            await CreateOrderLineTruck(truck1.Id, driver.Id, newOrder1Line1.Id, 1);

            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate1,
                DateEnd = newOrderDate2,
                OrderId = originalOrder.Id,
                CopyTrucks = true
            });
            newOrderIds.Length.ShouldBe(2);

            // Act
            var result = await _schedulingAppService.CopyOrdersTrucks(new CopyOrdersTrucksInput()
            {
                NewOrderIds = newOrderIds,
                OriginalOrderId = originalOrder.Id,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.ConflictingTrucks.ShouldNotBeNull();
            result.ConflictingTrucks.Count.ShouldBe(1);
            result.ConflictingTrucks.ShouldContain(truck1.TruckCode);
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => newOrderIds.Contains(olt.OrderLine.OrderId)).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }
        [Fact]
        public async Task Test_CopyOrdersTrucks_should_copy_trucks_when_Truck_is_in_use_and_ProceedOnConflict_is_true()
        {
            // Arrange
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

            DateTime newOrderDate1 = originalDate.AddDays(1);
            var newOrder1 = await CreateOrderWithOrderLines(newOrderDate1);
            var newOrder1Line1 = newOrder1.OrderLines.First(ol => ol.LineNumber == 1);
            await CreateOrderLineTruck(truck1.Id, driver.Id, newOrder1Line1.Id, 1);

            IOrderAppService orderAppService = Resolve<IOrderAppService>();
            var newOrderIds = await orderAppService.CopyOrder(new CopyOrderInput()
            {
                DateBegin = newOrderDate1,
                DateEnd = newOrderDate1,
                OrderId = originalOrder.Id,
                CopyTrucks = true,
            });
            newOrderIds.Length.ShouldBe(1);

            // Act
            var result = await _schedulingAppService.CopyOrdersTrucks(new CopyOrdersTrucksInput()
            {
                NewOrderIds = newOrderIds,
                OriginalOrderId = originalOrder.Id,
                ProceedOnConflict = true,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.ConflictingTrucks.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => newOrderIds.Contains(olt.OrderLine.OrderId)).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks[0].TruckId.ShouldBe(truck2.Id);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
