using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    public interface ITruckAppService : IApplicationService
    {
        Task<PagedResultDto<TruckDto>> GetTrucks(GetTrucksInput input);
        Task<PagedResultDto<SelectListDto>> GetTrucksSelectList(GetTrucksSelectListInput input);
        Task<IList<KeyValuePair<int, string>>> GetBedConstructionSelectList();
        Task<IList<KeyValuePair<int, string>>> GetFuelTypeSelectList();
        Task<TruckEditDto> GetTruckForEdit(GetTruckForEditInput input);
        Task<EditTruckResult> EditTruck(TruckEditDto model);
        Task<SetTruckIsOutOfServiceResult> SetTruckIsOutOfService(SetTruckIsOutOfServiceInput input);
        Task<bool> CanDeleteTruck(EntityDto input);
        Task DeleteTruck(EntityDto input);

        Task<AddSharedTruckListDto> GetAddSharedTruckModel(GetAddSharedTruckModelInput input);
        Task AddSharedTruck(AddSharedTruckInput input);
        Task DeleteSharedTruck(DeleteSharedTruckInput input);
        Task<TruckFileEditDto> SaveFile(TruckFileEditDto truckFileEditDto);
        Task<TruckFileEditDto> GetTruckFileEditDto(int id);
        Task RemoveDefaultDriver(int truckId, int driverId);
        Task RemoveTodayAndFutureDriverAssignmentsForTruckAndDriver(int truckId, int driverId, DateTime today);
        Task CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(int truckId, int driverId, DateTime today);
        Task UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput input);
        Task<string> GetTruckCodeByTruckId(int truckId);
    }
}
