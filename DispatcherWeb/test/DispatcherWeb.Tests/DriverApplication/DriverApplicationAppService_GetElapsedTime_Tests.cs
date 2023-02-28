using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverApplication
{
    public class DriverApplicationAppService_GetElapsedTime_Tests : DriverApplicationAppService_Tests_Base
    {
        public static object[][] DatesData =
        {
            new object[] {new (TimeSpan startSpan, TimeSpan endSpan)[] {(TimeSpan.FromMinutes(1 * 60), TimeSpan.FromMinutes(1 * 60 + 10))}},
            new object[] {new (TimeSpan startSpan, TimeSpan endSpan)[]
            {
                (TimeSpan.FromMinutes(1 * 60), TimeSpan.FromMinutes(1 * 60 + 10)),
                (TimeSpan.FromMinutes(2 * 60), TimeSpan.FromMinutes(2 * 60 + 20)),
                (TimeSpan.FromMinutes(3 * 60), TimeSpan.FromMinutes(3 * 60 + 30)),
            }},
        };
        [Theory, MemberData(nameof(DatesData))]
        public async Task Test_GetElapsedTime_should_return_ElapsedTime((TimeSpan startSpan, TimeSpan endSpan)[] records)
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            foreach (var record in records)
            {
                await CreateEmployeeTime(date.Add(record.startSpan), date.Add(record.endSpan), truck.Id);
            }

            // Act
            var result = await _driverApplicationAppService.GetElapsedTime();

            // Assert
            result.ElapsedTime.ShouldBe(date.AddSeconds(records.Sum(r => (r.endSpan - r.startSpan).TotalSeconds)));
            result.ClockIsStarted.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_GetElapsedTime_should_return_ElapsedTime_from_StartDateTime_to_Now()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            await CreateEmployeeTime(Clock.Now.AddHours(-1), null, truck.Id);

            // Act
            var result = await _driverApplicationAppService.GetElapsedTime();

            // Assert
            result.ElapsedTime.ShouldBeGreaterThanOrEqualTo(date.AddHours(1));
            result.ElapsedTime.ShouldBeLessThan(date.AddHours(1).AddSeconds(10));
            result.ClockIsStarted.ShouldBeTrue();
        }
    }
}
