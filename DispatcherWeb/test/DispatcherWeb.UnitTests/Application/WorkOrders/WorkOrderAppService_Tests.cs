﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using DispatcherWeb.PreventiveMaintenanceSchedule;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitTests.TestUtilities;
using DispatcherWeb.VehicleMaintenance;
using DispatcherWeb.WorkOrders;
using DispatcherWeb.WorkOrders.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.WorkOrders
{
    public class WorkOrderAppService_Tests
    {
        private int _truckId = 1;
        private readonly IWorkOrderAppService _workOrderAppService;
        private readonly IRepository<OutOfServiceHistory> _outOfServiceHistoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<WorkOrder> _workOrderRepository;
        private readonly IRepository<WorkOrderLine> _workOrderLineRepository;
        private IRepository<VehicleServiceType> _vehicleServiceTypeRepository;
        private IRepository<VehicleService> _vehicleServiceRepository;

        public WorkOrderAppService_Tests()
        {
            _workOrderRepository = Substitute.For<IRepository<WorkOrder>>();

            var workOrderLineData = new List<WorkOrderLine>
            {
                new WorkOrderLine
                {
                    WorkOrderId = 1,
                    VehicleService = new VehicleService
                    {
                        PreventiveMaintenance = new List<PreventiveMaintenance>
                        {
                            new PreventiveMaintenance { TruckId = _truckId },
                        }
                    }
                },
            }.AsQueryable();

            _workOrderLineRepository = Substitute.For<IRepository<WorkOrderLine>>();
            _workOrderLineRepository.GetAll().Returns(workOrderLineData);

            var workOrderPictureRepository = Substitute.For<IRepository<WorkOrderPicture>>();
            _outOfServiceHistoryRepository = Substitute.For<IRepository<OutOfServiceHistory>>();
            _truckRepository = Substitute.For<IRepository<Truck>>();
            var preventiveMaintenanceAppService = Substitute.For<IPreventiveMaintenanceAppService>();
            _vehicleServiceTypeRepository = Substitute.For<IRepository<VehicleServiceType>>();
            _vehicleServiceRepository = Substitute.For<IRepository<VehicleService>>();

            _workOrderAppService = new WorkOrderAppService(
                _workOrderRepository,
                _workOrderLineRepository,
                workOrderPictureRepository,
                _outOfServiceHistoryRepository,
                _truckRepository,
                preventiveMaintenanceAppService,
                _vehicleServiceRepository,
                _vehicleServiceTypeRepository
            );
        }

        [Fact]
        public async Task Test_SaveWorkOrder_should_not_create_OutOfServiceHistory_for_new_WorkOrder_anymore()
        {
            // Arrange
            var dto = GetNewWorkOrderEditDto();
            Truck truck = new Truck() { IsOutOfService = false };
            _truckRepository.GetAsync(_truckId).Returns(truck);

            // Act
            await _workOrderAppService.SaveWorkOrder(dto);

            // Assert
            await _outOfServiceHistoryRepository.DidNotReceive().InsertAsync(Arg.Is<OutOfServiceHistory>(x =>
                x.TruckId == _truckId &&
                x.OutOfServiceDate == dto.IssueDate &&
                x.Reason == $"Work Order for {dto.Note}"
            ));
            truck.IsOutOfService.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_SaveWorkOrder_should_not_create_OutOfServiceHistory_when_new_WorkOrder_and_Truck_IsOutOfService()
        {
            // Arrange
            var dto = GetNewWorkOrderEditDto();
            Truck truck = new Truck() { IsOutOfService = true };
            _truckRepository.GetAsync(_truckId).Returns(truck);

            // Act
            await _workOrderAppService.SaveWorkOrder(dto);

            // Assert
            await _outOfServiceHistoryRepository.DidNotReceive().InsertAsync(Arg.Any<OutOfServiceHistory>());
        }

        [Fact]
        public async Task Test_SaveWorkOrder_should_not_set_OutOfServiceHistory_InServiceDate_and_Truck_IsOutOfService_false_anymore_for_Completed_WorkOrder()
        {
            // Arrange
            var dto = GetNewWorkOrderEditDto(1);
            // WorkOrder is completed when previous Status was not Complete and a new Status is Complete
            dto.Status = WorkOrderStatus.Complete;
            _workOrderRepository.GetAsync(1).Returns(new WorkOrder() { Status = WorkOrderStatus.Pending });
            var workOrderMockSet = new List<WorkOrder>
            {
                new WorkOrder { Id = 1 },
            }.CreateMockSet();
            _workOrderRepository.GetAll().Returns<IQueryable<WorkOrder>>(workOrderMockSet);

            var mockSet = Substitute.For<DbSet<WorkOrderLine>, IQueryable<WorkOrderLine>, IAsyncEnumerable<WorkOrderLine>>().Initialize(new List<WorkOrderLine>().AsQueryable());
            _workOrderLineRepository.GetAll().Returns(mockSet);

            DateTime inServiceDate = new DateTime(2000, 1, 1);
            Truck truck = new Truck() { IsOutOfService = true, InServiceDate = inServiceDate };
            _truckRepository.GetAsync(_truckId).Returns(truck);

            // Act
            await _workOrderAppService.SaveWorkOrder(dto);

            // Assert
            truck.IsOutOfService.ShouldBeTrue();
            truck.InServiceDate.ShouldBe(inServiceDate);
        }

        [Fact]
        public async Task Test_SaveWorkOrder_should_not_set_OutOfServiceHistory_InServiceDate_and_Truck_IsOutOfService_false_when_there_is_another_WorkOrder()
        {
            // Arrange
            var dto = GetNewWorkOrderEditDto(1);
            // WorkOrder is completed when previous Status was not Complete and a new Status is Compete
            dto.Status = WorkOrderStatus.Complete;
            _workOrderRepository.GetAsync(1).Returns(new WorkOrder() { Status = WorkOrderStatus.Pending });

            var workOrderMockSet = new List<WorkOrder>
            {
                new WorkOrder { Id = 1 }, new WorkOrder { Id = 2 },
            }.CreateMockSet();
            _workOrderRepository.GetAll().Returns(workOrderMockSet);

            var workOrderLinesMockSet = Substitute.For<DbSet<WorkOrderLine>, IQueryable<WorkOrderLine>, IAsyncEnumerable<WorkOrderLine>>().Initialize(new List<WorkOrderLine>().AsQueryable());
            _workOrderLineRepository.GetAll().Returns(workOrderLinesMockSet);

            DateTime inServiceDate = new DateTime(2000, 1, 1);
            Truck truck = new Truck() { IsOutOfService = true, InServiceDate = inServiceDate };
            _truckRepository.GetAsync(_truckId).Returns(truck);

            // Act
            await _workOrderAppService.SaveWorkOrder(dto);

            // Assert
            truck.IsOutOfService.ShouldBeTrue();
            truck.InServiceDate.ShouldBe(inServiceDate);
        }

        [Fact]
        public async Task Test_CreateWorkOrdersFromPreventiveMaintenance_should_create_WorkOrders()
        {
            // Arrange
            var truckWithPreventiveMaintenance = new Truck
            {
                Id = _truckId,
                PreventiveMaintenances = new List<PreventiveMaintenance>
                {
                    new PreventiveMaintenance { Id = 11, VehicleServiceId = 21 },
                    new PreventiveMaintenance { Id = 12, VehicleServiceId = 22 },
                    new PreventiveMaintenance { Id = 13, VehicleServiceId = 23 },
                },
            };
            var trucksData = new List<Truck> { truckWithPreventiveMaintenance }.AsQueryable();
            var mockSet = Substitute.For<DbSet<Truck>, IQueryable<Truck>, IAsyncEnumerable<Truck>>().Initialize(trucksData);

            _truckRepository.GetAll().Returns(mockSet);
            _truckRepository.GetAsync(_truckId).Returns(truckWithPreventiveMaintenance);

            var vehicleServiceTypeData = (new List<VehicleServiceType>
            {
                new VehicleServiceType { Id = 1, Name = "ServiceType1" },
                new VehicleServiceType { Id = 2, Name = AppConsts.PreventiveMaintenanceServiceTypeName },
            }).AsQueryable();
            var mockSet2 = Substitute.For<DbSet<VehicleServiceType>, IQueryable<VehicleServiceType>, IAsyncEnumerable<VehicleServiceType>>().Initialize(vehicleServiceTypeData);
            _vehicleServiceTypeRepository.GetAll().Returns(mockSet2);

            // Act
            var input = new CreateWorkOrdersFromPreventiveMaintenanceInput
            {
                PreventiveMaintenanceIds = new[] { 11, 13 },
            };
            await _workOrderAppService.CreateWorkOrdersFromPreventiveMaintenance(input);

            // Assert
            await _workOrderRepository.Received().InsertOrUpdateAndGetIdAsync(Arg.Is<WorkOrder>(wo => wo.TruckId == 1 && wo.VehicleServiceTypeId == 2));
            await _workOrderLineRepository.Received().InsertOrUpdateAndGetIdAsync(Arg.Is<WorkOrderLine>(wol => wol.VehicleServiceId == 21));
            await _workOrderLineRepository.Received().InsertOrUpdateAndGetIdAsync(Arg.Is<WorkOrderLine>(wol => wol.VehicleServiceId == 23));
        }

        private WorkOrderEditDto GetNewWorkOrderEditDto(int id = 0)
        {
            DateTime issueDate = new DateTime(2018, 01, 01);
            _truckId = 1;
            string note = "WorkOrder Note";
            return new WorkOrderEditDto
            {
                Id = id,
                TruckId = _truckId,
                IssueDate = issueDate,
                Note = note,
            };

        }
    }
}
