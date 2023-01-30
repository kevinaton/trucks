using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleUsages.Dto;
using System;
using Abp.Linq.Extensions;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.VehicleUsages
{
    [AbpAuthorize(AppPermissions.Pages_VehicleUsages_View)]
    public class VehicleUsageAppService : DispatcherWebAppServiceBase, IVehicleUsageAppService
    {
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        public VehicleUsageAppService(IRepository<VehicleUsage> vehicleUsageRepository)
        {
            _vehicleUsageRepository = vehicleUsageRepository;
        }

        public async Task<PagedResultDto<VehicleUsageDto>> GetVehicleUsagePagedList(GetVehicleUsagePagedListInput input)
        {
            DateTime? utcDateTimeBegin = input.ReadingDateTimeBegin != null ? await ConvertFromLocalTimeZoneToUtc(input.ReadingDateTimeBegin.Value) : (DateTime?)null;
            DateTime? utcDateTimeEnd = input.ReadingDateTimeEnd != null ? await ConvertFromLocalTimeZoneToUtc(input.ReadingDateTimeEnd.Value) : (DateTime?)null;

            var query = _vehicleUsageRepository.GetAll()
                .WhereIf(input.OfficeId.HasValue, fp => fp.Truck.LocationId == input.OfficeId)
                .WhereIf(input.ReadingType.HasValue, fp => fp.ReadingType == input.ReadingType)
                .WhereIf(utcDateTimeBegin.HasValue, fp => fp.ReadingDateTime >= utcDateTimeBegin.Value)
                .WhereIf(utcDateTimeEnd.HasValue, fp => fp.ReadingDateTime < utcDateTimeEnd.Value.AddDays(1))
                .WhereIf(input.TruckId.HasValue, fp => fp.TruckId == input.TruckId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(fp => new VehicleUsageDto
                {
                    Id = fp.Id,
                    TruckCode = fp.Truck.TruckCode,
                    ReadingDateTime = fp.ReadingDateTime,
                    Reading = fp.Reading,
                    ReadingType = fp.ReadingType,
                    TruckId = fp.TruckId,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<VehicleUsageDto>(totalCount, items);
        }

        private async Task<DateTime> ConvertFromLocalTimeZoneToUtc(DateTime dateTime) =>
            dateTime.ConvertTimeZoneFrom(await GetTimezone());

        private async Task<DateTime> ConvertFromUtcToLocalTimeZone(DateTime dateTime) =>
            dateTime.ConvertTimeZoneTo(await GetTimezone());
        public async Task<VehicleUsageEditDto> GetVehicleUsageForEdit(NullableIdDto input)
        {
            VehicleUsageEditDto vehicleUsageEditDto;
            if (input.Id.HasValue)
            {
                vehicleUsageEditDto = await _vehicleUsageRepository.GetAll()
                    .Where(fp => fp.Id == input.Id)
                    .Select(fp => new VehicleUsageEditDto()
                    {
                        Id = fp.Id,
                        TruckId = fp.TruckId,
                        TruckCode = fp.Truck.TruckCode,
                        ReadingDateTime = fp.ReadingDateTime,
                        Reading = fp.Reading,
                        ReadingType = fp.ReadingType,
                    })
                    .FirstAsync();
                vehicleUsageEditDto.ReadingDateTime = await ConvertFromUtcToLocalTimeZone(vehicleUsageEditDto.ReadingDateTime);
            }
            else
            {
                vehicleUsageEditDto = new VehicleUsageEditDto();
            }
            return vehicleUsageEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleUsages_Edit)]
        public async Task<VehicleUsageEditDto> SaveVehicleUsage(VehicleUsageEditDto model)
        {
            VehicleUsage entity = model.Id != 0 ? await _vehicleUsageRepository.GetAsync(model.Id) : new VehicleUsage();
            await MapModelToEntity();
            model.Id = _vehicleUsageRepository.InsertOrUpdateAndGetId(entity);
            return model;

            // Local functions
            async Task MapModelToEntity()
            {
                entity.TruckId = model.TruckId;
                entity.ReadingDateTime = await ConvertFromLocalTimeZoneToUtc(model.ReadingDateTime);
                entity.Reading = model.Reading;
                entity.ReadingType = model.ReadingType;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleUsages_Edit)]
        public async Task DeleteVehicleUsage(EntityDto input) =>
            await _vehicleUsageRepository.DeleteAsync(input.Id);
    }
}
