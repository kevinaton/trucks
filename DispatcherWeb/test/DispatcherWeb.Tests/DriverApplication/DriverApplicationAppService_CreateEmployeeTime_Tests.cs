using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.DriverApplication.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverApplication
{
    public class DriverApplicationAppService_CreateEmployeeTime_Tests : DriverApplicationAppService_Tests_Base
    {
        [Fact]
        public async Task Test_CreateEmployeeTime_should_create_EmployeeTime_record()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            DateTime startTime1 = date.AddHours(12).AddMinutes(10);
            var truck = await CreateTruck();
            var truck2 = await CreateTruck();
            var order1 = await CreateOrderAndAssociateWithDriver(date, Shift.Shift1, startTime1, truck2, _driver);

            // Act
            await _driverApplicationAppService.CreateEmployeeTime(new CreateEmployeeTimeInput());

            // Assert
            var employeeTimeEntities = await UsingDbContextAsync(async context => await context.EmployeeTime.ToListAsync());
            employeeTimeEntities.Count.ShouldBe(1);
            var employeeTime = employeeTimeEntities[0];
            employeeTime.UserId.ShouldBe(_driverUser.Id);
            employeeTime.EquipmentId.ShouldBe(truck2.Id);
        }

        [Fact]
        public async Task Test_CreateEmployeeTime_should_create_EmployeeTime_record_and_set_EndDateTime_for_previous_one()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            DateTime startTime1 = date.AddHours(12).AddMinutes(10);
            var truck = await CreateTruck();
            var truck2 = await CreateTruck();
            var order1 = await CreateOrderAndAssociateWithDriver(date, Shift.Shift1, startTime1, truck2, _driver);
            var previousEmployeeTime = await CreateEmployeeTime(startTime1.AddDays(-1), null, truck2.Id);

            // Act
            await _driverApplicationAppService.CreateEmployeeTime(new CreateEmployeeTimeInput());

            // Assert
            var employeeTimeEntities = await UsingDbContextAsync(async context => await context.EmployeeTime.ToListAsync());
            employeeTimeEntities.Count.ShouldBe(2);
            var employeeTime = employeeTimeEntities.First(et => et.Id != previousEmployeeTime.Id);
            employeeTime.UserId.ShouldBe(_driverUser.Id);
            employeeTime.EquipmentId.ShouldBe(truck2.Id);
            employeeTime.StartDateTime.ShouldNotBe(new DateTime());
            employeeTime.EndDateTime.ShouldBeNull();

            var previousUpdatedEmployeeTime = employeeTimeEntities.First(et => et.Id == previousEmployeeTime.Id);
            previousUpdatedEmployeeTime.EndDateTime.ShouldNotBeNull();
            previousUpdatedEmployeeTime.EndDateTime.ShouldBe(startTime1.Date.AddMilliseconds(-1) /* The end of the previous day */);
        }

        [Fact]
        public async Task Test_CreateEmployeeTime_should_create_EmployeeTime_record_with_EquipmentId_null_when_there_there_is_no_DriverAssignment()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            DateTime startTime1 = date.AddHours(12).AddMinutes(10);

            // Act
            await _driverApplicationAppService.CreateEmployeeTime(new CreateEmployeeTimeInput());

            // Assert
            var employeeTimeEntities = await UsingDbContextAsync(async context => await context.EmployeeTime.ToListAsync());
            employeeTimeEntities.Count.ShouldBe(1);
            var employeeTime = employeeTimeEntities[0];
            employeeTime.UserId.ShouldBe(_driverUser.Id);
            employeeTime.EquipmentId.ShouldBeNull();
        }

        [Fact]
        public async Task Test_CreateEmployeeTime_should_not_create_new_EmployeeTime_record_when_there_is_one_for_driver()
        {
            // Arrange
            DateTime date = DateTime.UtcNow.Date;
            DateTime startTime1 = date.AddHours(12).AddMinutes(10);
            var truck = await CreateTruck();
            var truck2 = await CreateTruck();
            var order1 = await CreateOrderAndAssociateWithDriver(date, Shift.Shift1, startTime1, truck2, _driver);
            await _driverApplicationAppService.CreateEmployeeTime(new CreateEmployeeTimeInput()); // Create one EmployeeTime record

            // Act
            await _driverApplicationAppService.CreateEmployeeTime(new CreateEmployeeTimeInput());

            // Assert
            var employeeTimeEntities = await UsingDbContextAsync(async context => await context.EmployeeTime.ToListAsync());
            employeeTimeEntities.Count.ShouldBe(1);
        }

    }
}
