using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_UpdateDispatchTicket_Tests : DispatchingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_UpdateDispatchTicket_should_create_Ticket()
        {
            // Arrange
            var coords = new { Lat = 50.6995393, Long = 28.6414887 };
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            order = await UpdateEntity(order, o => o.Shift = Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);

            // Act 
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.UpdateDispatchTicket(new DispatchTicketDto()
                {
                    Guid = dispatch.Guid,
                    TicketNumber = "N001",
                    MaterialAmount = 11.11m,
                    SourceLatitude = coords.Lat,
                    SourceLongitude = coords.Long,
                });
            }

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Loaded);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.SourceDateTime.ShouldNotBeNull();
            load.SourceLatitude.ShouldBe(coords.Lat);
            load.SourceLongitude.ShouldBe(coords.Long);
            var ticket = load.Tickets.FirstOrDefault();
            ticket.ShouldNotBeNull();
            ticket.OfficeId.ShouldBe(_officeId);
            ticket.TenantId.ShouldBe(1);
            ticket.OrderLineId.ShouldBe(orderLine.Id);
            ticket.CustomerId.ShouldBe(order.CustomerId);
            ticket.DriverId.ShouldBe(driver.Id);
            ticket.TruckId.ShouldBe(truck.Id);
            ticket.ServiceId.ShouldBe(orderLine.ServiceId);
            ticket.TicketNumber.ShouldBe("N001");
            ticket.Quantity.ShouldBe(11.11m);
            ticket.UnitOfMeasureId.ShouldBe(1);
            ticket.TicketDateTime.ShouldBe(today);
            ticket.Shift.ShouldBeNull();
        }

        [Fact]
        public async Task Test_UpdateDispatchTicket_should_update_existing_Load_and_Ticket()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            var ticketEntity = await CreateTicketForDispatch(dispatch, order, truck, orderLine);
            var loads = await UsingDbContextAsync(async context => await context.Loads.Where(l => l.DispatchId == dispatch.Id).ToListAsync());
            loads.Count.ShouldBe(1);

            // Act 
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.UpdateDispatchTicket(new DispatchTicketDto()
                {
                    Guid = dispatch.Guid,
                    LoadId = loads[0].Id,
                    TicketNumber = "N001",
                    MaterialAmount = 11.11m,
                });
            }

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Loaded);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.SourceDateTime.ShouldNotBeNull();
            var ticket = load.Tickets.FirstOrDefault();
            ticket.Id.ShouldBe(ticketEntity.Id);
            ticket.ShouldNotBeNull();
            ticket.OfficeId.ShouldBe(_officeId);
            ticket.TenantId.ShouldBe(1);
            ticket.OrderLineId.ShouldBe(orderLine.Id);
            ticket.CustomerId.ShouldBe(order.CustomerId);
            ticket.DriverId.ShouldBe(driver.Id);
            ticket.TruckId.ShouldBe(truck.Id);
            ticket.ServiceId.ShouldBe(orderLine.ServiceId);
            ticket.TicketNumber.ShouldBe("N001");
            ticket.Quantity.ShouldBe(11.11m);
            ticket.UnitOfMeasureId.ShouldBe(1);
            ticket.TicketDateTime.ShouldBe(today);
        }

        [Fact]
        public async Task Test_UpdateDispatchTicket_should_create_new_Load_and_Ticket()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            var ticketEntity = await CreateTicketForDispatch(dispatch, order, truck, orderLine);

            // Act 
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.UpdateDispatchTicket(new DispatchTicketDto()
                {
                    Guid = dispatch.Guid,
                    TicketNumber = "N001",
                    MaterialAmount = 11.11m,
                });
            }

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Loaded);
            updatedDispatch.Loads.Count.ShouldBe(2);
            var load = updatedDispatch.Loads.OrderByDescending(l => l.Id).First();
            load.SourceDateTime.ShouldNotBeNull();
            var ticket = load.Tickets.FirstOrDefault();
            ticket.Id.ShouldNotBe(ticketEntity.Id);
            ticket.ShouldNotBeNull();
            ticket.OfficeId.ShouldBe(_officeId);
            ticket.TenantId.ShouldBe(1);
            ticket.OrderLineId.ShouldBe(orderLine.Id);
            ticket.CustomerId.ShouldBe(order.CustomerId);
            ticket.DriverId.ShouldBe(driver.Id);
            ticket.TruckId.ShouldBe(truck.Id);
            ticket.ServiceId.ShouldBe(orderLine.ServiceId);
            ticket.TicketNumber.ShouldBe("N001");
            ticket.Quantity.ShouldBe(11.11m);
            ticket.UnitOfMeasureId.ShouldBe(1);
            ticket.TicketDateTime.ShouldBe(today);
        }

        [Fact]
        public async Task Test_UpdateDispatchTicket_should_not_create_Ticket_when_DispatchVia_setting_is_SimplifiedSms()
        {
            // Arrange
            var coords = new { Lat = 50.6995393, Long = 28.6414887 };
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            order = await UpdateEntity(order, o => o.Shift = Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            ((AbpServiceBase)_dispatchingAppService).SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(DispatchVia.SimplifiedSms.ToIntString());

            // Act 
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.UpdateDispatchTicket(new DispatchTicketDto()
                {
                    Guid = dispatch.Guid,
                    TicketNumber = "N001",
                    MaterialAmount = 11.11m,
                    SourceLatitude = coords.Lat,
                    SourceLongitude = coords.Long,
                });
            }

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Loaded);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.SourceDateTime.ShouldNotBeNull();
            load.SourceLatitude.ShouldBe(coords.Lat);
            load.SourceLongitude.ShouldBe(coords.Long);
            load.Tickets.Any().ShouldBeFalse();
        }

        [Fact]
        public async Task Test_UpdateDispatchTicket_should_update_existing_empty_Load_if_it_exists()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            await CreateLoad(dispatch.Id);

            // Act 
            using (UsingTenantId(null))
            {
                await _dispatchingAppService.UpdateDispatchTicket(new DispatchTicketDto()
                {
                    Guid = dispatch.Guid,
                    TicketNumber = "N001",
                    MaterialAmount = 11.11m,
                });
            }

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Loaded);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.SourceDateTime.ShouldNotBeNull();
            var ticket = load.Tickets.FirstOrDefault();
            ticket.ShouldNotBeNull();
            ticket.Id.ShouldNotBe(0);
            ticket.OfficeId.ShouldBe(_officeId);
            ticket.TenantId.ShouldBe(1);
            ticket.OrderLineId.ShouldBe(orderLine.Id);
            ticket.CustomerId.ShouldBe(order.CustomerId);
            ticket.DriverId.ShouldBe(driver.Id);
            ticket.TruckId.ShouldBe(truck.Id);
            ticket.ServiceId.ShouldBe(orderLine.ServiceId);
            ticket.TicketNumber.ShouldBe("N001");
            ticket.Quantity.ShouldBe(11.11m);
            ticket.UnitOfMeasureId.ShouldBe(1);
            ticket.TicketDateTime.ShouldBe(today);
        }


    }
}
