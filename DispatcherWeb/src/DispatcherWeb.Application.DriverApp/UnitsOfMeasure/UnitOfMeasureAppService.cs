using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using DispatcherWeb.DriverApp.UnitsOfWork.Dto;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.UnitsOfWork
{
    [AbpAuthorize]
    public class UnitOfMeasureAppService : DispatcherWebDriverAppAppServiceBase, IUnitOfMeasureAppService
    {
        private readonly IRepository<UnitOfMeasure> _uomRepository;

        public UnitOfMeasureAppService(
            IRepository<UnitOfMeasure> uomRepository
            )
        {
            _uomRepository = uomRepository;
        }

        public async Task<IListResult<UnitOfMeasureDto>> Get()
        {
            var result = await _uomRepository.GetAll()
                .Select(x => new UnitOfMeasureDto
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .OrderBy(x => x.Id)
                .ToListAsync();

            return new ListResultDto<UnitOfMeasureDto>(result);
        }
    }
}
