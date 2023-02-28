using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using DispatcherWeb.Dto;
using DispatcherWeb.Services;

namespace DispatcherWeb.UnitsOfMeasure
{
    [AbpAuthorize]
    public class UnitOfMeasureAppService : DispatcherWebAppServiceBase, IUnitOfMeasureAppService
    {
        private readonly IRepository<UnitOfMeasure> _unitOfMeasureRepository;
        private readonly IRepository<OfficeServicePrice> _servicePriceRepository;

        public UnitOfMeasureAppService(
            IRepository<UnitOfMeasure> unitOfMeasureRepository,
            IRepository<OfficeServicePrice> servicePriceRepository
            )
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _servicePriceRepository = servicePriceRepository;
        }

        public async Task<PagedResultDto<SelectListDto>> GetUnitsOfMeasureSelectList(GetSelectListInput input)
        {
            var query = _unitOfMeasureRepository.GetAll()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                });

            return await query.GetSelectListResult(input);
        }

        //public async Task<ListResultDto<SelectListDto>> GetUomsForService(NullableIdDto input)
        //{
        //    if (input.Id == null)
        //    {
        //        return new ListResultDto<SelectListDto>();
        //    }
        //    var serviceUoms = await _servicePriceRepository.GetAll()
        //        .Where(x => x.ServiceId == input.Id && x.OfficeId == OfficeId)
        //        .OrderBy(x => x.Id)
        //        .Select(x => x.UnitOfMeasure)
        //        .Distinct()
        //        .Select(x => new SelectListDto
        //        {
        //            Id = x.Id.ToString(),
        //            Name = x.Name
        //        })
        //        .ToListAsync();
        //    return new ListResultDto<SelectListDto>(serviceUoms);
        //}
    }
}
