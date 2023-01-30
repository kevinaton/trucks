using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.FuelPurchases.Dto;
using DispatcherWeb.Trucks;
using DispatcherWeb.WorkOrders.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.FuelPurchases
{
    [AbpAuthorize(AppPermissions.Pages_FuelPurchases_View)]
    public class FuelPurchaseAppService : DispatcherWebAppServiceBase, IFuelPurchaseAppService
    {
        private readonly IRepository<FuelPurchase> _fuelPurchaseRepository;

        public FuelPurchaseAppService(
            IRepository<FuelPurchase> fuelPurchaseRepository
        )
        {
            _fuelPurchaseRepository = fuelPurchaseRepository;
        }

        public async Task<PagedResultDto<FuelPurchaseDto>> GetFuelPurchasePagedList(GetFuelPurchasePagedListInput input)
        {
            DateTime? utcDateTimeBegin = input.FuelDateTimeBegin != null ? await ConvertFromLocalTimeZoneToUtc(input.FuelDateTimeBegin.Value) : (DateTime?)null;
            DateTime? utcDateTimeEnd = input.FuelDateTimeEnd != null ? await ConvertFromLocalTimeZoneToUtc(input.FuelDateTimeEnd.Value) : (DateTime?)null;

            var query = _fuelPurchaseRepository.GetAll()
                .WhereIf(input.OfficeId.HasValue, fp => fp.Truck.LocationId == input.OfficeId)
                .WhereIf(utcDateTimeBegin.HasValue, fp => fp.FuelDateTime >= utcDateTimeBegin.Value)
                .WhereIf(utcDateTimeEnd.HasValue, fp => fp.FuelDateTime < utcDateTimeEnd.Value.AddDays(1))
                .WhereIf(input.TruckId.HasValue, fp => fp.TruckId == input.TruckId)
                ;

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(fp => new FuelPurchaseDto
                {
                    Id = fp.Id, TruckCode = fp.Truck.TruckCode, FuelDateTime = fp.FuelDateTime,
                    Amount = fp.Amount,
                    Rate = fp.Rate,
                    Odometer = fp.Odometer,
                    TicketNumber = fp.TicketNumber,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<FuelPurchaseDto>(totalCount, items);
        }

        private async Task<DateTime> ConvertFromLocalTimeZoneToUtc(DateTime dateTime) => 
            dateTime.ConvertTimeZoneFrom(await GetTimezone());

        private async Task<DateTime> ConvertFromUtcToLocalTimeZone(DateTime dateTime) =>
            dateTime.ConvertTimeZoneTo(await GetTimezone());

        public async Task<FuelPurchaseEditDto> GetFuelPurchaseForEdit(NullableIdDto input)
        {
            FuelPurchaseEditDto fuelPurchaseEditDto;
            if (input.Id.HasValue)
            {
                fuelPurchaseEditDto = await _fuelPurchaseRepository.GetAll()
                    .Where(fp => fp.Id == input.Id)
                    .Select(fp => new FuelPurchaseEditDto()
                    {
                        Id = fp.Id,
                        TruckId = fp.TruckId,
                        TruckCode = fp.Truck.TruckCode,
                        FuelDateTime = fp.FuelDateTime,
                        Amount = fp.Amount,
                        Odometer = fp.Odometer,
                        Rate = fp.Rate,
                        TicketNumber = fp.TicketNumber,
                    })
                    .FirstAsync();
                fuelPurchaseEditDto.FuelDateTime = await ConvertFromUtcToLocalTimeZone(fuelPurchaseEditDto.FuelDateTime);
            }
            else
            {
                fuelPurchaseEditDto = new FuelPurchaseEditDto();
            }
            return fuelPurchaseEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_FuelPurchases_Edit)]
        public async Task<FuelPurchaseEditDto> SaveFuelPurchase(FuelPurchaseEditDto model)
        {
            FuelPurchase entity = model.Id != 0 ? await _fuelPurchaseRepository.GetAsync(model.Id) : new FuelPurchase();
            await MapModelToEntity();
            model.Id = _fuelPurchaseRepository.InsertOrUpdateAndGetId(entity);
            return model;

            // Local functions
            async Task MapModelToEntity()
            {
                entity.TruckId = model.TruckId;
                entity.FuelDateTime = await ConvertFromLocalTimeZoneToUtc(model.FuelDateTime);
                entity.Amount = model.Amount;
                entity.Odometer = model.Odometer;
                entity.Rate = model.Rate;
                entity.TicketNumber = model.TicketNumber;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_FuelPurchases_Edit)]
        public async Task DeleteFuelPurchase(EntityDto input) => 
            await _fuelPurchaseRepository.DeleteAsync(input.Id);
    }
}
