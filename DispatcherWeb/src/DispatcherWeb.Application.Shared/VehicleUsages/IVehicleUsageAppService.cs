using Abp.Application.Services.Dto;
using DispatcherWeb.VehicleUsages.Dto;
using System.Threading.Tasks;

namespace DispatcherWeb.VehicleUsages
{
    public interface IVehicleUsageAppService
    {
        Task<PagedResultDto<VehicleUsageDto>> GetVehicleUsagePagedList(GetVehicleUsagePagedListInput input);
        Task<VehicleUsageEditDto> SaveVehicleUsage(VehicleUsageEditDto model);
        Task<VehicleUsageEditDto> GetVehicleUsageForEdit(NullableIdDto input);
        Task DeleteVehicleUsage(EntityDto input);
    }
}
