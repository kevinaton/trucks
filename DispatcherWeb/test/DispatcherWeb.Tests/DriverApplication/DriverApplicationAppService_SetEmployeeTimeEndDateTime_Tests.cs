using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverApplication
{
    public class DriverApplicationAppService_SetEmployeeTimeEndDateTime_Tests : DriverApplicationAppService_Tests_Base
    {
        [Fact]
        public async Task Test_SetEmployeeTimeEndDateTime_should_update_EmployeeTime_EndDateTime()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            await CreateEmployeeTime(date, null, truck.Id);

            // Act
            await _driverApplicationAppService.SetEmployeeTimeEndDateTime();

            // Assert
            var employeeTimes = await UsingDbContextAsync(async context => await context.EmployeeTime.ToListAsync());
            employeeTimes.Count.ShouldBe(1);
            employeeTimes[0].EndDateTime.ShouldNotBeNull();
        }
    }
}
