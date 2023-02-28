using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders.RevenueBreakdownByTruckReport
{
    public class RevenueBreakdownByTruckReportByOrderLinesDataService_Tests : AppTestBase, IAsyncLifetime
    {
        private int _officeId;
        private IRevenueBreakdownByTruckReportDataService _revenueBreakdownByTruckReportDataService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _revenueBreakdownByTruckReportDataService = Resolve<IRevenueBreakdownByTruckReportByOrderLinesDataService>();

        }

        [Fact]
        public async Task Test_GetRevenueBreakdownItems_should_return_one_item_for_each_Truck()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var order1 = await CreateOrderWithOrderLines(date);
            var orderLine11 = order1.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine12 = order1.OrderLines.First(ol => ol.LineNumber == 2);
            var truck1 = await CreateTruck();
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine11.Id, .5m);
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine12.Id, .5m);

            // Act
            var items = await _revenueBreakdownByTruckReportDataService.GetRevenueBreakdownItems(new RevenueBreakdownByTruckReportInput()
            {
                DeliveryDateBegin = date,
                DeliveryDateEnd = date,
                OfficeId = _officeId,
            });

            // Assert
            items.Count.ShouldBe(1);
            items[0].Truck.ShouldBe(truck1.TruckCode);
            items[0].MaterialRevenue.ShouldBe(orderLine11.MaterialPricePerUnit * 10 + orderLine12.MaterialPricePerUnit * 20 ?? 0);
            items[0].FreightRevenue.ShouldBe(orderLine11.FreightPricePerUnit * 10 + orderLine12.FreightPricePerUnit * 20 ?? 0);
        }

        [Fact]
        public async Task Test_GetRevenueBreakdownItems_should_round_Material_and_Freight_to_2_digits_when_price_is_overriden()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var order1 = await CreateOrderWithOrderLines(date);
            var orderLine11 = order1.OrderLines.First(ol => ol.LineNumber == 1);
            await UpdateEntity(orderLine11, ol =>
            {
                ol.IsFreightPriceOverridden = true;
                ol.FreightPrice = 5.6789m;
                ol.IsMaterialPriceOverridden = true;
                ol.MaterialPrice = 15.6789m;
            });
            var truck1 = await CreateTruck();
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine11.Id, 1);

            // Act
            var items = await _revenueBreakdownByTruckReportDataService.GetRevenueBreakdownItems(new RevenueBreakdownByTruckReportInput()
            {
                DeliveryDateBegin = date,
                DeliveryDateEnd = date,
                OfficeId = _officeId,
            });

            // Assert
            items.Count.ShouldBe(1);
            items[0].Truck.ShouldBe(truck1.TruckCode);
            items[0].FreightRevenue.ShouldBe(5.68m);
            items[0].MaterialRevenue.ShouldBe(15.68m);
        }

        [Fact]
        public async Task Test_GetRevenueBreakdownItems_should_round_Material_and_Freight_to_2_digits_when_price_is_not_overriden()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var order1 = await CreateOrderWithOrderLines(date);
            var orderLine11 = order1.OrderLines.First(ol => ol.LineNumber == 1);
            await UpdateEntity(orderLine11, ol =>
            {
                ol.IsFreightPriceOverridden = false;
                ol.FreightPricePerUnit = 5.6789m;
                ol.IsMaterialPriceOverridden = false;
                ol.MaterialPricePerUnit = 15.6789m;
            });
            var truck1 = await CreateTruck();
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine11.Id, 1);

            // Act
            var items = await _revenueBreakdownByTruckReportDataService.GetRevenueBreakdownItems(new RevenueBreakdownByTruckReportInput()
            {
                DeliveryDateBegin = date,
                DeliveryDateEnd = date,
                OfficeId = _officeId,
            });

            // Assert
            items.Count.ShouldBe(1);
            items[0].Truck.ShouldBe(truck1.TruckCode);
            items[0].FreightRevenue.ShouldBe(56.79m);
            items[0].MaterialRevenue.ShouldBe(156.79m);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
