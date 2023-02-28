using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverApplication
{
    public class DriverApplicationAppService_UserIsClockedIn_Tests : DriverApplicationAppService_Tests_Base
    {
        [Fact]
        public async Task Test_UserIsClockedIn_should_return_true_when_there_is_EmployeeTime_record_with_EndDateTime_null()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            await CreateEmployeeTime(date, null, truck.Id);

            // Act
            var result = await _driverApplicationAppService.UserIsClockedIn();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_UserIsClockedIn_should_return_false_when_there_is_no_EmployeeTime_record_with_EndDateTime_null()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            await CreateEmployeeTime(date, date.AddHours(1), truck.Id);

            // Act
            var result = await _driverApplicationAppService.UserIsClockedIn();

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_UserIsClockedIn_should_return_false_when_there_is_no_any_EmployeeTime_record()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;

            // Act
            var result = await _driverApplicationAppService.UserIsClockedIn();

            // Assert
            result.ShouldBeFalse();
        }
    }
}
