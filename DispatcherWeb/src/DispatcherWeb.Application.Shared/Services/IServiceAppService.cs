using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Services.Dto;

namespace DispatcherWeb.Services
{
    public interface IServiceAppService : IApplicationService
    {
        Task<PagedResultDto<ServiceDto>> GetServices(GetServicesInput input);
        Task<PagedResultDto<SelectListDto>> GetServicesSelectList(GetSelectListInput input);
        Task<ListResultDto<SelectListDto>> GetServicesByIdsSelectList(GetItemsByIdsInput input);
        Task<ServicePricingDto> GetServicePricing(GetServicePricingInput input);
        Task<ServiceEditDto> GetServiceForEdit(NullableIdNameDto input);
        Task<int> EditService(ServiceEditDto model);
        Task<bool> CanDeleteService(EntityDto input);
        Task DeleteService(EntityDto input);

        Task<PagedResultDto<ServicePriceDto>> GetServicePrices(GetServicePricesInput input);
        Task<ServicePriceEditDto> GetServicePriceForEdit(NullableIdDto input);
        Task EditServicePrice(ServicePriceEditDto model);
        Task<bool> CanDeleteServicePrice(EntityDto input);
        Task DeleteServicePrice(EntityDto input);

        Task MergeServices(DataMergeInput input);
    }
}
