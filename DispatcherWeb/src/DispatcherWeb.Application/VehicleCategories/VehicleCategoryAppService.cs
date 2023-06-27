﻿using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleCategories.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.VehicleCategories
{
    [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
    public class VehicleCategoryAppService : DispatcherWebAppServiceBase, IVehicleCategoryAppService
    {
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<Truck> _truckRepository;

        public VehicleCategoryAppService(IRepository<VehicleCategory> vehicleCategoryRepository, IRepository<Truck> truckRepository)
        {
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _truckRepository = truckRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task<bool> CanDeleteVehicleCategory(EntityDto input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                return !await _truckRepository.GetAll().AnyAsync(p => p.VehicleCategoryId == input.Id);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task DeleteVehicleCategory(EntityDto input)
        {
            var canDelete = await CanDeleteVehicleCategory(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _vehicleCategoryRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task<VehicleCategoryEditDto> EditVehicleCategory(VehicleCategoryEditDto model)
        {
            var vehicleCategory = model.Id.HasValue ? await _vehicleCategoryRepository.GetAsync(model.Id.Value) : new VehicleCategory();

            vehicleCategory.Name = model.Name;
            vehicleCategory.AssetType = model.AssetType;
            vehicleCategory.IsPowered = model.IsPowered;
            vehicleCategory.SortOrder = model.SortOrder.Value;

            model.Id = await _vehicleCategoryRepository.InsertOrUpdateAndGetIdAsync(vehicleCategory);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task<VehicleCategoryEditDto> GetVehicleCategoryForEdit(NullableIdDto input)
        {
            VehicleCategoryEditDto vehicleCategoryEditDto;

            if (input.Id.HasValue)
            {
                vehicleCategoryEditDto = await _vehicleCategoryRepository.GetAll()
                    .Select(l => new VehicleCategoryEditDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        AssetType = l.AssetType,
                        IsPowered = l.IsPowered,
                        SortOrder = l.SortOrder
                    })
                    .FirstAsync(s => s.Id == input.Id.Value);
            }
            else
            {
                vehicleCategoryEditDto = new VehicleCategoryEditDto
                {
                };
            }

            return vehicleCategoryEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task<PagedResultDto<VehicleCategoryDto>> GetVehicleCategories(GetVehicleCategoriesInput input)
        {
            var query = _vehicleCategoryRepository.GetAll()
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.Contains(input.Name))
                .WhereIf(input.AssetType.HasValue, x => x.AssetType == input.AssetType.Value)
                .WhereIf(input.IsPowered.HasValue, x => x.IsPowered == input.IsPowered.Value)
                .Select(x => new VehicleCategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    AssetType = x.AssetType,
                    AssetTypeName = x.AssetType.ToString(),
                    IsPowered = x.IsPowered,
                    SortOrder = x.SortOrder,
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<VehicleCategoryDto>(
                totalCount, 
                items);
        }
    }
}
