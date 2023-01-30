using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Trucks
{
    public class TruckTelematicsAppService_Tests : AppTestBase, IAsyncLifetime
    {
        private ITruckTelematicsAppService _truckTelematicsAppService;
        private ITelematics _telematics;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _telematics = Substitute.For<ITelematics>();
            List<TruckCurrentData> truckCurrentDataList = new List<TruckCurrentData>
            {
                new TruckCurrentData { TruckCodeOrUniqueId = "101", CurrentMileage = 1000, CurrentHours = 2000 },
            };
            _telematics.GetCurrentDataForAllTrucksAsync().Returns(Task.FromResult(truckCurrentDataList.AsEnumerable()));
            _truckTelematicsAppService = Resolve<ITruckTelematicsAppService>(new { telematics = _telematics });
            ((DispatcherWebAppServiceBase)_truckTelematicsAppService).Session = CreateSession();

        }

        [Fact]
        public async Task Test_UpdateMileage_should_update_CurrentMileage_and_CurrentHours_of_Truck_and_create_VehicleUsage_record()
        {
            // Arrange
            var truck = await CreateTruck("101");

            // Act
            var result = await _truckTelematicsAppService.UpdateMileageForTenantAsync(1, 1);

            // Assert
            result.trucksUpdated.ShouldBe(1);
            result.trucksIgnored.ShouldBe(0);
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentMileage.ShouldBe(1000);
            updatedTruck.CurrentHours.ShouldBe(2000);

            var vehicleUsages = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            vehicleUsages.Count.ShouldBe(2);
            vehicleUsages[0].TruckId.ShouldBe(truck.Id);
            vehicleUsages[1].TruckId.ShouldBe(truck.Id);
            vehicleUsages.First(vu => vu.ReadingType == ReadingType.Miles).Reading.ShouldBe(1000);
            vehicleUsages.First(vu => vu.ReadingType == ReadingType.Hours).Reading.ShouldBe(2000);
        }

        [Fact]
        public async Task Test_UpdateMileage_should_not_update_wrong_Truck()
        {
            // Arrange
            var truck = await CreateTruck("404");

            // Act
            var result = await _truckTelematicsAppService.UpdateMileageForTenantAsync(1, 1);

            // Assert
            result.trucksUpdated.ShouldBe(0);
            result.trucksIgnored.ShouldBe(1);
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentMileage.ShouldBe(0);
            updatedTruck.CurrentHours.ShouldBe(0);

            var vehicleUsages = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            vehicleUsages.Count.ShouldBe(0);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
