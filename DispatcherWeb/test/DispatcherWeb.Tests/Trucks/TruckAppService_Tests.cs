using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.Tests.TestInfrastructure;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Trucks
{
    public class TruckAppService_Tests : AppTestBase
    {
        private readonly ITruckAppService _truckAppService;

        public TruckAppService_Tests()
        {
            _truckAppService = Resolve<ITruckAppService>();
            ((DispatcherWebAppServiceBase)_truckAppService).Session = CreateSession();
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_delete_truck_from_OrderLines_for_the_date()
        {
            var truckEntity = await CreateTruckEntity();
            var today = DateTime.Today;
            await CreateOrder(truckEntity, today);

            await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput()
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truckEntity.Id,
            });

            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(ol => ol.IsDeleted == false).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_not_delete_truck_from_OrderLines_for_past_date()
        {
            var truckEntity = await CreateTruckEntity();
            var today = DateTime.Today;
            await CreateOrder(truckEntity, today.AddDays(-1));

            await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput()
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truckEntity.Id,
            });

            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(ol => ol.IsDeleted == false).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_cancel_Unacknowledged_Dispatches()
        {
            var truck = await CreateTruckEntity();
            var today = DateTime.Today;
            var order = await CreateOrder(truck, today);
            var orderLine = order.OrderLines.First();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);

            // Act
            var result = await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput()
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truck.Id,
            });

            // Assert
            result.ThereWereCanceledDispatches.ShouldBeTrue();
            result.ThereWereNotCanceledDispatches.ShouldBeFalse();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeFalse();
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch2.Id));
            updatedDispatch2.IsDeleted.ShouldBeFalse();
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Canceled);
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_not_cancel_Acknowledged_or_Loaded_Dispatches()
        {
            var truck = await CreateTruckEntity();
            var today = DateTime.Today;
            var order = await CreateOrder(truck, today);
            var orderLine = order.OrderLines.First();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act
            var result = await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput()
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truck.Id,
            });

            // Assert
            result.ThereWereCanceledDispatches.ShouldBeFalse();
            result.ThereWereNotCanceledDispatches.ShouldBeTrue();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeFalse();
            updatedDispatch.Status.ShouldBe(DispatchStatus.Acknowledged);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch2.Id));
            updatedDispatch2.IsDeleted.ShouldBeFalse();
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Loaded);
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_true_when_there_are_no_dependencies()
        {
            var truckEntity = await CreateTruckEntity();

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_Order_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateOrder(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_AddSharedTruck_should_throw_UserFriendlyException_when_truck_is_scheduled_for_Date()
        {
            // Arrange
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruckEntity();
            var order = await CreateOrder(truckEntity, today);
            var office = await CreateOffice();

            // Act, Assert
            await _truckAppService.AddSharedTruck(new AddSharedTruckInput()
            {
                TruckId = truckEntity.Id,
                OfficeId = office.Id,
                StartDate = today,
                EndDate = today,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_AddSharedTruck_should_throw_UserFriendlyException_when_truck_is_scheduled_for_Date_and_Shift()
        {
            // Arrange
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruckEntity();
            var order = await CreateOrder(truckEntity, today);
            order = await UpdateEntity(order, o => o.Shift = Shift.Shift1);
            ((AbpServiceBase)_truckAppService).SubstituteSetting(AppSettings.General.UseShifts, "true");
            var office = await CreateOffice();

            // Act, Assert
            await _truckAppService.AddSharedTruck(new AddSharedTruckInput()
            {
                TruckId = truckEntity.Id,
                OfficeId = office.Id,
                StartDate = today,
                EndDate = today,
                Shifts = new[] { Shift.Shift1 },
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_AddSharedTruck_should_share_truck_when_truck_is_not_scheduled_for_Shift()
        {
            // Arrange
            DateTime today = Clock.Now.Date;
            var truckEntity = await CreateTruckEntity();
            var order = await CreateOrder(truckEntity, today);
            order = await UpdateEntity(order, o => o.Shift = Shift.Shift1);
            ((AbpServiceBase)_truckAppService).SubstituteSetting(AppSettings.General.UseShifts, "true");
            var office = await CreateOffice();

            // Act
            await _truckAppService.AddSharedTruck(new AddSharedTruckInput()
            {
                TruckId = truckEntity.Id,
                OfficeId = office.Id,
                StartDate = today,
                EndDate = today,
                Shifts = new[] { Shift.Shift2 },
            });

            // Assert
            var sharedTrucks = await UsingDbContextAsync(async context => await context.SharedTrucks.ToListAsync());
            sharedTrucks.Count.ShouldBe(1);
            var sharedTruck = sharedTrucks[0];
            sharedTruck.TruckId.ShouldBe(truckEntity.Id);
            sharedTruck.OfficeId.ShouldBe(office.Id);
            sharedTruck.Date.ShouldBe(today);
            sharedTruck.Shift.ShouldBe(Shift.Shift2);
        }

        private async Task<Order> CreateOrder(Truck truckEntity, DateTime? orderDate = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                Order order = new Order()
                {
                    TenantId = 1,
                    Customer = new Customer() { TenantId = 1, Name = "Cust" },
                    LocationId = truckEntity.Office.Id,
                    DeliveryDate = orderDate,
                };
                var orderLine = new OrderLine()
                {
                    TenantId = 1,
                    Service = new Service()
                    {
                        TenantId = 1,
                        Service1 = "sss",
                    },
                    MaterialUomId = 1,
                };
                order.OrderLines.Add(orderLine);
                orderLine = new OrderLine()
                {
                    TenantId = 1,
                    Service = new Service()
                    {
                        TenantId = 1,
                        Service1 = "sss",
                    },
                    MaterialUomId = 1,
                };
                order.OrderLines.Add(orderLine);
                orderLine.OrderLineTrucks.Add(new OrderLineTruck()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    DriverId = truckEntity.DefaultDriverId
                });

                await context.Orders.AddAsync(order);
                return order;
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_Ticket_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateTicket(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateTicket(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                Ticket ticket = new Ticket()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                };
                await context.Tickets.AddAsync(ticket);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_true_when_Ticket_exists_and_Carrier_is_not_null()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateTicketWithCarrier(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeTrue();
        }

        private async Task CreateTicketWithCarrier(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                Ticket ticket = new Ticket()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    Carrier = new LeaseHauler() { TenantId = 1, Name = "Carrier" },
                };
                await context.Tickets.AddAsync(ticket);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_PreventiveMaintenance_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreatePreventiveMaintenance(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreatePreventiveMaintenance(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                PreventiveMaintenance preventiveMaintenance = new PreventiveMaintenance()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    VehicleService = new VehicleService()
                    {
                        TenantId = 1,
                        Name = "VS1",
                    }
                };
                await context.PreventiveMaintenance.AddAsync(preventiveMaintenance);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_DriverAssignment_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateDriverAssignment(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateDriverAssignment(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                DriverAssignment driverAssignment = new DriverAssignment()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    OfficeId = truckEntity.LocationId,
                };
                await context.DriverAssignments.AddAsync(driverAssignment);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_SharedTruck_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateSharedTruck(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateSharedTruck(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                SharedTruck sharedTruck = new SharedTruck()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    OfficeId = truckEntity.LocationId ?? 0,
                };
                await context.SharedTrucks.AddAsync(sharedTruck);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_WorkOrder_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateWorkOrder(truckEntity);

            bool canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateWorkOrder(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                WorkOrder workOrder = new WorkOrder()
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                };
                await context.WorkOrders.AddAsync(workOrder);
            });
        }

        private async Task<Truck> CreateTruckEntity()
        {
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var office = new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" };
                // Create several trucks so the Id wouldn't be always equal to 1
                var vehicleCategory = await CreateVehicleCategory();
                Truck truck = new Truck()
                {
                    TenantId = 1,
                    TruckCode = "101",
                    Office = office,
                    VehicleCategory = vehicleCategory,
                };
                await context.Trucks.AddAsync(truck);
                truck = new Truck()
                {
                    TenantId = 1,
                    TruckCode = "102",
                    Office = office,
                    VehicleCategory = vehicleCategory,
                };
                await context.Trucks.AddAsync(truck);

                truck = new Truck()
                {
                    TenantId = 1,
                    TruckCode = "001",
                    Office = office,
                    VehicleCategory = vehicleCategory,
                };
                await context.Trucks.AddAsync(truck);

                return truck;
            });
            return truckEntity;
        }

    }
}
