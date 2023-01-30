using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Timing;
using DispatcherWeb.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.FuelPurchases
{
    public class FuelPurchaseAppService_GetFuelPurchaseForEdit_Tests : FuelPurchaseAppService_Tests_Base
    {
        [Fact]
        public async Task Test_GetFuelPurchaseForEdit_should_return_FuelPurchaseEditDto_with_local_FuelDateTime()
        {
            // Arrange
            DateTime date = new DateTime(2019, 8, 8);
            var truck = await CreateTruck();
            var fuelPurchase = await CreateFuelPurchase(truck.Id, date, 1, 1, 1, "1");
            ((DispatcherWebAppServiceBase)_fuelPurchaseAppService).SubstituteSetting(TimingSettingNames.TimeZone, "India Standard Time");

            // Act
            var result = await _fuelPurchaseAppService.GetFuelPurchaseForEdit(new NullableIdDto(fuelPurchase.Id));

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(fuelPurchase.Id);
            result.FuelDateTime.ShouldBe(date.AddHours(5).AddMinutes(30));
        }

    }
}
