using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.Timing;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.Drivers;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.Trucks.Exporting;
using DispatcherWeb.UnitTests.TestUtilities;
using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.Trucks
{
    public class TruckAppService_Tests
    {
        private readonly int _truckId = 1;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OutOfServiceHistory> _outOfServiceHistoryRepository;
        private readonly TruckAppService _truckAppService;
        private readonly DateTime _now = ClockProvider.DateTimeNow;

        public TruckAppService_Tests()
        {
            _truckRepository = Substitute.For<IRepository<Truck>>();
            var vehicleCategoryRepository = Substitute.For<IRepository<VehicleCategory>>();
            IRepository<TruckFile> truckFileRepository = Substitute.For<IRepository<TruckFile>>();
            //var truckFilesMockSet = new List<TruckFile>{}.CreateMockSet();
            truckFileRepository.GetAllListAsync(x => true).ReturnsForAnyArgs(new List<TruckFile> { });
            IRepository<SharedTruck> sharedTruckRepository = Substitute.For<IRepository<SharedTruck>>();
            IRepository<Order> orderRepository = Substitute.For<IRepository<Order>>();
            IRepository<OrderLineTruck> orderLineTruckRepository = Substitute.For<IRepository<OrderLineTruck>>();
            var mockSet = Substitute.For<DbSet<OrderLineTruck>, IQueryable<OrderLineTruck>, IAsyncEnumerable<OrderLineTruck>>().Initialize(new List<OrderLineTruck>().AsQueryable());
            orderLineTruckRepository.GetAll().Returns(mockSet);
            IRepository<Office> officeRepository = Substitute.For<IRepository<Office>>();
            _outOfServiceHistoryRepository = Substitute.For<IRepository<OutOfServiceHistory>>();
            IRepository<DriverAssignment> driverAssignmentsRepository = Substitute.For<IRepository<DriverAssignment>>();
            IRepository<Dispatch> dispatchRepository = Substitute.For<IRepository<Dispatch>>();
            var dispatchMockSet = Substitute.For<DbSet<Dispatch>, IQueryable<Dispatch>, IAsyncEnumerable<Dispatch>>().Initialize(new List<Dispatch>().AsQueryable());
            dispatchRepository.GetAll().Returns(dispatchMockSet);
            IRepository<Ticket> ticketRepository = Substitute.For<IRepository<Ticket>>();
            IRepository<TimeOff> timeOffRepository = Substitute.For<IRepository<TimeOff>>();
            IOrderLineUpdaterFactory orderLineUpdaterFactory = Substitute.For<IOrderLineUpdaterFactory>();
            ISingleOfficeAppService singleOfficeAppService = Substitute.For<ISingleOfficeAppService>();
            ITruckListCsvExporter truckListCsvExporter = Substitute.For<ITruckListCsvExporter>();
            ISyncRequestSender syncRequestSender = Substitute.For<ISyncRequestSender>();
            IDriverApplicationPushSender driverApplicationPushSender = Substitute.For<IDriverApplicationPushSender>();

            _truckAppService = new TruckAppService(
                _truckRepository,
                vehicleCategoryRepository,
                truckFileRepository,
                sharedTruckRepository,
                orderRepository,
                orderLineTruckRepository,
                officeRepository,
                _outOfServiceHistoryRepository,
                driverAssignmentsRepository,
                dispatchRepository,
                ticketRepository,
                timeOffRepository,
                orderLineUpdaterFactory,
                singleOfficeAppService,
                truckListCsvExporter,
                driverApplicationPushSender,
                syncRequestSender,
                Substitute.For<IEmailSender>(),
                Substitute.For<IAppNotifier>(),
                Substitute.For<ICrossTenantOrderSender>()
            );
            ((DispatcherWebAppServiceBase)_truckAppService).Session = SessionHelper.CreateSession();
            ISettingManager settingManager = Substitute.For<ISettingManager>();
            settingManager.GetSettingValueAsync(TimingSettingNames.TimeZone).Returns("UTC");
            _truckAppService.SettingManager = settingManager;
        }

        [Fact(Skip = "Error")]
        public async Task Test_EditTruck_should_create_OutOfServiceHistory_when_IsOutOfService_was_false_and_set_true()
        {
            // Arrange

            var dto = new TruckEditDto { Id = _truckId, IsOutOfService = true, OfficeId = 1, Files = new List<TruckFileEditDto>(), IsActive = true };
            Truck truck = new Truck() { Id = _truckId, IsOutOfService = false, LocationId = 1 };
            _truckRepository.GetAsync(_truckId).Returns(truck);

            // Act
            await _truckAppService.EditTruck(dto);

            // Assert
            truck.IsOutOfService.ShouldBeTrue();
            truck.OutOfServiceHistories.ShouldNotBeNull();
            truck.OutOfServiceHistories.Count.ShouldBe(1);
            var outOfServiceHistory = truck.OutOfServiceHistories.First();
            outOfServiceHistory.OutOfServiceDate.ShouldBe(_now);
        }

        [Fact(Skip = "Error")]
        public async Task Test_EditTruck_should_update_OutOfServiceHistory_InServiceDate_when_IsOutOfService_was_true_and_set_false()
        {
            // Arrange
            var dto = new TruckEditDto { Id = _truckId, IsOutOfService = false, OfficeId = 1, Files = new List<TruckFileEditDto>(), IsActive = true, };
            Truck truck = new Truck() { Id = _truckId, IsOutOfService = true, LocationId = 1 };
            _truckRepository.GetAsync(_truckId).Returns(truck);
            var outOfServiceHistory = new OutOfServiceHistory();
            _outOfServiceHistoryRepository.FirstOrDefaultAsync(x => true).ReturnsForAnyArgs(outOfServiceHistory);

            // Act
            await _truckAppService.EditTruck(dto);

            // Assert
            truck.IsOutOfService.ShouldBeFalse();
            outOfServiceHistory.InServiceDate.ShouldBe(_now);
        }

        [Fact(Skip = "Error")]
        public async Task Test_EditTruck_should_not_create_OutOfServiceHistory_when_IsOutOfService_was_not_changed()
        {
            // Arrange
            var dto = new TruckEditDto { Id = _truckId, IsOutOfService = false, OfficeId = 1, Files = new List<TruckFileEditDto>(), IsActive = true, };
            Truck truck = new Truck() { Id = _truckId, IsOutOfService = false, LocationId = 1 };
            _truckRepository.GetAsync(_truckId).Returns(truck);

            // Act
            await _truckAppService.EditTruck(dto);

            // Assert
            truck.IsOutOfService.ShouldBeFalse();
            truck.OutOfServiceHistories.ShouldNotBeNull();
            truck.OutOfServiceHistories.Count.ShouldBe(0);
        }

    }
}
