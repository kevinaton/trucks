using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.UnitsOfMeasure
{
    public interface IUnitOfMeasureAppService : IApplicationService
    {
        Task<PagedResultDto<SelectListDto>> GetUnitsOfMeasureSelectList(GetSelectListInput input);
        //Task<ListResultDto<SelectListDto>> GetUomsForService(NullableIdDto input);
    }
}
