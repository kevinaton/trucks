using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Locations.Dto;

namespace DispatcherWeb.Locations
{
    public interface ILocationAppService : IApplicationService
    {
        Task<PagedResultDto<LocationDto>> GetLocations(GetLocationsInput input);
        Task<PagedResultDto<SelectListDto>> GetLocationsSelectList(GetSelectListInput input);
        Task<ListResultDto<SelectListDto>> GetLocationsByIdsSelectList(GetItemsByIdsInput input);
        Task<LocationEditDto> GetLocationForEdit(GetLocationForEditInput input);
        Task<LocationEditDto> EditLocation(LocationEditDto model);
        Task<bool> CanDeleteLocation(EntityDto input);
        Task DeleteLocation(EntityDto input);

        Task<PagedResultDto<SupplierContactDto>> GetSupplierContacts(GetSupplierContactsInput input);
        Task<ListResultDto<SelectListDto>> GetSupplierContactsByIdsSelectList(GetItemsByIdsInput input);
        Task<SupplierContactEditDto> GetSupplierContactForEdit(NullableIdDto input);
        Task EditSupplierContact(SupplierContactEditDto model);
        Task DeleteSupplierContact(EntityDto input);

        Task MergeLocations(DataMergeInput input);
        Task MergeSupplierContacts(DataMergeInput input);
    }
}
