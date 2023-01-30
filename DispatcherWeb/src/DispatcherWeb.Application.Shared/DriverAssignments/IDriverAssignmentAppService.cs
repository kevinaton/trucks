﻿using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.DriverAssignments.Dto;

namespace DispatcherWeb.DriverAssignments
{
    public interface IDriverAssignmentAppService : IApplicationService
    {
        Task<ListResultDto<DriverAssignmentDto>> GetDriverAssignments(GetDriverAssignmentsInput input);
        Task<SetNoDriverForTruckResult> SetNoDriverForTruck(SetNoDriverForTruckInput input);
        Task EditDriverAssignment(DriverAssignmentEditDto input);
        Task<bool> OrderLineTruckExists(OrderLineTruckExistsInput input);
        Task SetDefaultDriverForTruck(SetDefaultDriverForTruckInput input);
        Task<byte[]> GetDriverAssignmentReport(GetDriverAssignmentsInput input);
        Task SetDriverForTruck(SetDriverForTruckInput input);
		Task<ThereAreOpenDispatchesForTruckOnDateResult> ThereAreOpenDispatchesForTruckOnDate(ThereAreOpenDispatchesForTruckOnDateInput input);
		Task<int> AddUnscheduledTrucks(AddUnscheduledTrucksInput input);
        Task AddDefaultStartTime(AddDefaultStartTimeInput input);
    }
}
