using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.FuelSurchargeCalculations.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    [AbpAuthorize]
    public class FuelSurchargeCalculationAppService : DispatcherWebAppServiceBase, IFuelSurchargeCalculationAppService
    {
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        public FuelSurchargeCalculationAppService(
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository
            )
        {
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
        }

        public async Task<List<FuelSurchargeCalculationEditDto>> GetFuelSurchargeCalculations()
        {
            var items = await _fuelSurchargeCalculationRepository.GetAll()
                .Select(x => new FuelSurchargeCalculationEditDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    BaseFuelCost = x.BaseFuelCost,
                    CanChangeBaseFuelCost = x.CanChangeBaseFuelCost,
                    Increment = x.Increment,
                    FreightRatePercent = x.FreightRatePercent,
                    Credit = x.Credit
                }).ToListAsync();

            return items;
        }

        public async Task<FuelSurchargeCalculationEditDto> EditFuelSurchargeCalculation(FuelSurchargeCalculationEditDto model)
        {
            var entity = model.Id == 0 ? new FuelSurchargeCalculation() : await _fuelSurchargeCalculationRepository.GetAsync(model.Id);

            entity.Name = model.Name;
            entity.BaseFuelCost = model.BaseFuelCost;
            entity.CanChangeBaseFuelCost = model.CanChangeBaseFuelCost;
            entity.Increment = model.Increment;
            entity.FreightRatePercent = model.FreightRatePercent;
            entity.Credit = model.Credit;

            if (entity.Id == 0)
            {
                model.Id = await _fuelSurchargeCalculationRepository.InsertAndGetIdAsync(entity);
            }

            return model;
        }

        public async Task DeleteFuelSurchargeCalculation(EntityDto model)
        {
            if (model.Id == await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.DefaultFuelSurchargeCalculationId))
            {
                throw new UserFriendlyException(L("CannotDeleteDefaultFuelSurchargeCalculation"));
            }

            var item = await _fuelSurchargeCalculationRepository.GetAll()
                .Where(x => x.Id == model.Id)
                .Select(x => new
                {
                    HasQuotes = x.Quotes.Any(),
                    HasOrders = x.Orders.Any()
                }).FirstAsync();

            if (item.HasQuotes || item.HasOrders)
            {
                throw new UserFriendlyException(L("CannotDeleteFuelSurchargeCalculation"));
            }

            await _fuelSurchargeCalculationRepository.DeleteAsync(model.Id);
        }

        public async Task SetDefaultFuelSurchargeCalculationId(int? id)
        {
            id ??= 0;
            await SettingManager.ChangeSettingForTenantAsync(Session.GetTenantId(), AppSettings.Fuel.DefaultFuelSurchargeCalculationId, id.ToString());
        }

        public async Task<PagedResultDto<SelectListDto>> GetFuelSurchargeCalculationsSelectList(GetFuelSurchargeCalculationsSelectListInput input)
        {
            var query = _fuelSurchargeCalculationRepository.GetAll()
                .WhereIf(input.CanChangeBaseFuelCost.HasValue, x => x.CanChangeBaseFuelCost == input.CanChangeBaseFuelCost)
                .Select(x => new SelectListDto<FuelSurchargeCalculationSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new FuelSurchargeCalculationSelectListInfoDto
                    {
                        CanChangeBaseFuelCost = x.CanChangeBaseFuelCost,
                        BaseFuelCost = x.BaseFuelCost
                    }
                });

            var result = await query.GetSelectListResult(input);

            return result;
        }
    }
}
