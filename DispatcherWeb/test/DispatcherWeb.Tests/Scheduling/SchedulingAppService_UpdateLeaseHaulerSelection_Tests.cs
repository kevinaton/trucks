using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_UpdateLeaseHaulerSelection_Tests : SchedulingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_UpdateLeaseHaulerSelection_should_delete_LeaseHauler_OrderLineTrucks()
        {
            // Arrange
            DateTime date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First();
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerTruck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            var leaseHaulerDriver = await CreateLeaseHaulerDriver(leaseHauler.Id);
            var availableLeaseHaulerTruck = await CreateAvailableLeaseHaulerTruck(leaseHauler.Id, leaseHaulerTruck.Id, leaseHaulerDriver.Id, date, Shift.Shift1, _officeId);
            var leaseHaulerOrderLineTruck = await CreateOrderLineTruck(leaseHaulerTruck.Id, leaseHaulerDriver.Id, orderLine1.Id, 1);

            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            var model = new LeaseHaulerSelectionDto()
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
                Rows = new List<LeaseHaulerSelectionRowDto>(),
            };

            // Act
            await _schedulingAppService.UpdateLeaseHaulerSelection(model);

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.ToListAsync());
            orderLineTrucks.Count.ShouldBe(2);
            orderLineTrucks.First(olt => olt.Id == leaseHaulerOrderLineTruck.Id).IsDeleted.ShouldBeTrue();
            orderLineTrucks.First(olt => olt.Id == orderLineTruck.Id).IsDeleted.ShouldBeFalse();
        }
    }
}
