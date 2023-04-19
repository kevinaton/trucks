using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.VehicleCategories.Dto;
using System.Threading.Tasks;

namespace DispatcherWeb.VehicleCategories
{
    public interface IVehicleCategoryAppService : IApplicationService
    {
        Task<PagedResultDto<VehicleCategoryDto>> GetVehicleCategories(GetVehicleCategoriesInput input);
        Task<VehicleCategoryEditDto> GetVehicleCategoryForEdit(NullableIdDto input);
        Task<VehicleCategoryEditDto> EditVehicleCategory(VehicleCategoryEditDto model);
        Task<bool> CanDeleteVehicleCategory(EntityDto input);
        Task DeleteVehicleCategory(EntityDto input);
    }
}
