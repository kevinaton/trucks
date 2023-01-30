using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.FuelPurchases;
using DispatcherWeb.FuelPurchases.Dto;
using DispatcherWeb.Trucks;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.FuelPurchases
{
    public class FuelPurchaseAppService_GetFuelPurchasePagedList_Tests : AppTestBase, IAsyncLifetime
    {
        private IFuelPurchaseAppService _fuelPurchaseAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _fuelPurchaseAppService = Resolve<IFuelPurchaseAppService>();
            ((DispatcherWebAppServiceBase)_fuelPurchaseAppService).Session = CreateSession();

        }

        [Fact]
        public async Task Test_GetFuelPurchasePagedList_should_return_FuelPurchasePagedList()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            var fuelPurchase = await CreateFuelPurchase(truck.Id, date, 1, 1, 1, "1");

            // Act
            var result = await _fuelPurchaseAppService.GetFuelPurchasePagedList(new GetFuelPurchasePagedListInput());

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Items[0].TruckCode.ShouldBe("101");
            result.Items[0].FuelDateTime.ShouldBe(date);
            result.Items[0].Amount.ShouldBe(1);
            result.Items[0].Rate.ShouldBe(1);
            result.Items[0].Odometer.ShouldBe(1);
            result.Items[0].TicketNumber.ShouldBe("1");
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
