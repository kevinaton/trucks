using System;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Customers;
using DispatcherWeb.Dashboard;
using DispatcherWeb.Dashboard.Dto;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard
{
    public class DashboardAppService_Tests : AppTestBase, IAsyncLifetime
    {
        private IDashboardAppService _dashboardAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _dashboardAppService = Resolve<IDashboardAppService>();
            ((DispatcherWebAppServiceBase)_dashboardAppService).Session = CreateSession();
        }

        [Fact]
        public async Task Test_GetTruckUtilizationData_should_return_data_by_day()
        {
            // Arrange
            DateTime periodBegin = new DateTime(2019, 3, 1);
            DateTime periodEnd = new DateTime(2019, 3, 31);
            await CreateTrucks();

            // Act
            var result = await _dashboardAppService.GetTruckUtilizationData(new GetTruckUtilizationDataInput
            {
                DatePeriod = TruckUtilizationDatePeriod.Daily,
                PeriodBegin = periodBegin,
                PeriodEnd = periodEnd,
            });

            // Assert
            var truckUtilizationData = result.TruckUtilizationData;
            truckUtilizationData.Count.ShouldBe(31);
            truckUtilizationData[0].UtilizationPercent.ShouldBe((int)Math.Round(5m / 9 * 100, 0));
            truckUtilizationData[1].UtilizationPercent.ShouldBe((int)Math.Round(9m / 9 * 100, 0));
            truckUtilizationData[2].UtilizationPercent.ShouldBe((int)Math.Round(0m / 9 * 100, 0));
            truckUtilizationData[9].UtilizationPercent.ShouldBe((int)Math.Round(5m / 10 * 100, 0));
            truckUtilizationData[19].UtilizationPercent.ShouldBe((int)Math.Round(5m / 9 * 100, 0));
            truckUtilizationData[24].UtilizationPercent.ShouldBe((int)Math.Round(5m / 8 * 100, 0));
            truckUtilizationData[30].UtilizationPercent.ShouldBe((int)Math.Round(1m / 8 * 100, 0));
        }

        [Fact]
        public async Task Test_GetTruckUtilizationData_should_return_data_by_week()
        {
            // Arrange
            DateTime periodBegin = new DateTime(2019, 3, 1);
            DateTime periodEnd = new DateTime(2019, 3, 31);
            await CreateTrucks();

            // Act
            var result = await _dashboardAppService.GetTruckUtilizationData(new GetTruckUtilizationDataInput
            {
                DatePeriod = TruckUtilizationDatePeriod.Weekly,
                PeriodBegin = periodBegin,
                PeriodEnd = periodEnd,
            });

            // Assert
            var truckUtilizationData = result.TruckUtilizationData;
            truckUtilizationData.Count.ShouldBe(6);
            truckUtilizationData[0].UtilizationPercent.ShouldBe((int)Math.Round((5m + 9m) / (9 * 2) * 100, 0));
            truckUtilizationData[1].UtilizationPercent.ShouldBe(0);
            truckUtilizationData[2].UtilizationPercent.ShouldBe((int)Math.Round(5m / 70 * 100, 0));
            truckUtilizationData[3].UtilizationPercent.ShouldBe((int)Math.Round(5m / 66 * 100, 0));
            truckUtilizationData[4].UtilizationPercent.ShouldBe((int)Math.Round((5m) / 56 * 100, 0));
            truckUtilizationData[5].UtilizationPercent.ShouldBe((int)Math.Round(1m / 8 * 100, 0));
        }

        [Fact]
        public async Task Test_GetTruckUtilizationData_should_return_data_by_year()
        {
            // Arrange
            DateTime periodBegin = new DateTime(2019, 3, 1);
            DateTime periodEnd = new DateTime(2019, 3, 31);
            await CreateTrucks();

            // Act
            var result = await _dashboardAppService.GetTruckUtilizationData(new GetTruckUtilizationDataInput
            {
                DatePeriod = TruckUtilizationDatePeriod.Monthly,
                PeriodBegin = periodBegin,
                PeriodEnd = periodEnd,
            });

            // Assert
            var truckUtilizationData = result.TruckUtilizationData;
            truckUtilizationData.Count.ShouldBe(1);
            truckUtilizationData[0].UtilizationPercent.ShouldBe((int)Math.Round((5m + 9m + 5m + 5m + 5m + 1m) / (9 * 21 + 10 * 5 + 9 * 5) * 100, 0));
        }

        [Fact]
        public async Task Test_GetTruckUtilizationData_should_return_100_percent_utilization_for_truck_with_2_OrderLines_worked()
        {
            // Arrange
            DateTime periodBegin = new DateTime(2019, 3, 1);
            DateTime periodEnd = new DateTime(2019, 3, 1);
            var order = await CreateOrderWithOrderLines(periodBegin);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, .5m);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, .5m);

            // Act
            var result = await _dashboardAppService.GetTruckUtilizationData(new GetTruckUtilizationDataInput
            {
                DatePeriod = TruckUtilizationDatePeriod.Daily,
                PeriodBegin = periodBegin,
                PeriodEnd = periodEnd,
            });

            // Assert
            var truckUtilizationData = result.TruckUtilizationData;
            truckUtilizationData.Count.ShouldBe(1);
            truckUtilizationData[0].UtilizationPercent.ShouldBe(100);
        }

        private async Task CreateTrucks()
        {
            int[] deliveriesPerDay = { 5, 9, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 1, };
            int numberOfOrderLineTrucksToCreate = deliveriesPerDay.Sum();
            await UsingDbContextAsync(async context =>
            {
                var dumpTrucks = await CreateVehicleCategory(new VehicleCategory { AssetType = AssetType.DumpTruck, IsPowered = true, Name = "DumpTruck", SortOrder = 1 });
                var trailers = await CreateVehicleCategory(new VehicleCategory { AssetType = AssetType.Trailer, IsPowered = false, Name = "Trailer", SortOrder = 2 });
                for (int i = 1; i <= 10; i++)
                {
                    var truck = CreateTruckInstance($"{i}", dumpTrucks);
                    switch (i)
                    {
                        case 1:
                            truck.InServiceDate = new DateTime(2019, 3, 10);
                            break;
                        case 2:
                            truck.InactivationDate = new DateTime(2019, 3, 20);
                            break;
                        case 3:
                            truck.SoldDate = new DateTime(2019, 3, 25);
                            break;
                    }
                    for (int d = 1; d <= 31; d++)
                    {
                        DateTime orderDate = new DateTime(2019, 3, d);
                        if (truck.PurchaseDate > orderDate || truck.InactivationDate <= orderDate ||
                           deliveriesPerDay[d - 1]-- <= 0)
                        {
                            continue;
                        }
                        var orderLineTruck = CreateOrderLineTruckInstance(orderDate);
                        if (orderDate == new DateTime(2019, 3, 31))
                        {
                            // Order from another office
                            orderLineTruck.OrderLine.Order.Office = new Office() { TenantId = 1, Name = "office2", TruckColor = "fff" };
                        }
                        truck.OrderLineTrucks.Add(orderLineTruck);
                    }
                    await context.Trucks.AddAsync(truck);
                }

                // These trucks shouldn't change result
                await context.Trucks.AddAsync(CreateTruckInstance("101", dumpTrucks, new DateTime(2020, 1, 1)));
                await context.Trucks.AddAsync(CreateTruckInstance("102", dumpTrucks, new DateTime(2019, 1, 1), new DateTime(2019, 3, 1)));
                await context.Trucks.AddAsync(CreateTruckInstance("103", dumpTrucks, soldDate: new DateTime(2018, 1, 1)));
                await context.Trucks.AddAsync(CreateTruckInstance("104", trailers));
                var anotherOfficeTruck = CreateTruckInstance("1001", dumpTrucks);
                anotherOfficeTruck.Office = new Office() { TenantId = 1, Name = "office3", TruckColor = "fff" };
                await context.Trucks.AddAsync(anotherOfficeTruck);

                Truck CreateTruckInstance(string truckCode, VehicleCategory vehicleCategory, DateTime? purchaseDate = null, DateTime? inactivationDate = null, DateTime? soldDate = null) =>
                    new Truck()
                    {
                        TenantId = 1,
                        TruckCode = truckCode,
                        VehicleCategory = vehicleCategory,
                        LocationId = _officeId,
                        IsActive = true,
                        IsOutOfService = false,
                        InServiceDate = purchaseDate ?? new DateTime(2019, 1, 1),
                        InactivationDate = inactivationDate,
                        SoldDate = soldDate,
                    };
                OrderLineTruck CreateOrderLineTruckInstance(DateTime date)
                {
                    Order order = new Order
                    {
                        TenantId = 1,
                        DeliveryDate = date,
                        Shift = Shift.Shift1,
                        Customer = context.Customers.FirstOrDefault() ?? new Customer() { TenantId = 1, Name = "Cust" },
                        LocationId = _officeId,
                        SalesTaxRate = 2,
                    };
                    OrderLine orderLine = new OrderLine()
                    {
                        TenantId = 1,
                        Order = order,
                        LineNumber = 1,
                        Designation = DesignationEnum.FreightAndMaterial,
                        MaterialQuantity = 1,
                        FreightQuantity = 1,
                        FreightPricePerUnit = 2,
                        MaterialPricePerUnit = 3,
                        Service = new Service()
                        {
                            TenantId = 1,
                            Service1 = "sss",
                        },
                        MaterialUomId = 1,
                        FreightUomId = 1,
                        NumberOfTrucks = 2,
                    };
                    OrderLineTruck orderLineTruck = new OrderLineTruck
                    {
                        TenantId = 1,
                        OrderLine = orderLine,
                        DriverId = null,
                        IsDone = true,
                        Utilization = 0,
                    };
                    return orderLineTruck;
                }

            });
            int orderLineTruckCount = await UsingDbContextAsync(async context => await context.OrderLineTrucks.CountAsync());
            orderLineTruckCount.ShouldBe(numberOfOrderLineTrucksToCreate);
        }



        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
