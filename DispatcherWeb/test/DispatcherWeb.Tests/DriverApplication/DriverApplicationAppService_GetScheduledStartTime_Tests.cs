using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverApplication
{
    public class DriverApplicationAppService_GetScheduledStartTime_Tests : DriverApplicationAppService_Tests_Base
    {

        [Fact]
        public async Task Test_GetScheduledStartTime_should_return_earliest_StartTime_of_all_associated_Orders()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            DateTime startTime1 = date.AddHours(12).AddMinutes(10);
            DateTime startTime2 = date.AddHours(11).AddMinutes(20);
            var truck = await CreateTruck();
            var order1 = await CreateOrderAndAssociateWithDriver(date, Shift.Shift1, startTime1, truck, _driver);
            var order2 = await CreateOrderAndAssociateWithDriver(date, Shift.Shift1, startTime2, truck, _driver);

            // Act
            var result = await _driverApplicationAppService.GetScheduledStartTime();

            // Assert
            result.ShouldBe(startTime2);
        }

        [Fact]
        public async Task Test_GetScheduledStartTime_should_return_null_when_there_is_no_associated_Orders()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;

            // Act
            var result = await _driverApplicationAppService.GetScheduledStartTime();

            // Assert
            result.ShouldBeNull();
        }
    }
}
