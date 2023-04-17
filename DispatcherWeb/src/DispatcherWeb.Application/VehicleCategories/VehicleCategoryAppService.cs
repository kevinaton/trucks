using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleCategories.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

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
        public async Task<bool> CanDeleteVehicleCategory(EntityDto input) => 
            await _truckRepository.FirstOrDefaultAsync(p => p.VehicleCategoryId == input.Id) == null;

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
            vehicleCategory.SortOrder = model.SortOrder;

            model.Id = await _vehicleCategoryRepository.InsertOrUpdateAndGetIdAsync(vehicleCategory);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task<VehicleCategoryEditDto> GetVehicleCategoryForEdit(GetVehicleCategoryForEditInput input)
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
                    .SingleAsync(s => s.Id == input.Id.Value);
            }
            else
            {
                vehicleCategoryEditDto = new VehicleCategoryEditDto
                {
                    Name = input.Name,
                };
            }

            return vehicleCategoryEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleCategories)]
        public async Task<PagedResultDto<VehicleCategoryDto>> GetVehicleCategories(GetVehicleCategoriesInput input)
        {
            var query = GetFilteredVehicleCategoryQuery(input);

            var totalCount = await query.CountAsync();

            var items = await GetVehicleCategoryDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<VehicleCategoryDto>(
                totalCount,
                items);
        }

        [HttpGet]
        public async Task<ListResultDto<SelectListDto>> GetVehicleCategoriesByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = (await _vehicleCategoryRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto<VehicleCategorySelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new VehicleCategorySelectListInfoDto
                    {
                        Name = x.Name,
                        AssetType = x.AssetType,
                        IsPowered = x.IsPowered,
                        SortOrder = x.SortOrder
                    }
                })
                .OrderBy(x => x.Name)
                .ToListAsync())
                .Select(x => new SelectListDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();

            return new ListResultDto<SelectListDto>(items);
        }

        [HttpGet]
        public async Task<ListResultDto<SelectListDto>> GetAssetTypesSelectList()
        {
            var results = await SelectListExtensions.GetEnumItemsSelectList<AssetType>();
            return new ListResultDto<SelectListDto>(results);
        }

        public async Task<PagedResultDto<SelectListDto>> GetVehicleCategoriesSelectList(GetSelectListInput input)
        {
            var query = _vehicleCategoryRepository.GetAll()
                            .Select(x => new SelectListDto<VehicleCategorySelectListInfoDto>
                            {
                                Id = x.Id.ToString(),
                                Name = x.Name,
                                Item = new VehicleCategorySelectListInfoDto
                                {
                                    Name = x.Name,
                                    AssetType = x.AssetType,
                                    IsPowered = x.IsPowered,
                                    SortOrder = x.SortOrder
                                }
                            });

            return await query.GetSelectListResult(input);
        }

        #region private methods

        private IQueryable<VehicleCategory> GetFilteredVehicleCategoryQuery(IGetVehicleCategoryFilteredList input)
        {
            var query = _vehicleCategoryRepository.GetAll()
                    .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.ToLower().Contains(input.Name.ToLower()))
                    .WhereIf(input.AssetType.HasValue, x => x.AssetType == input.AssetType.Value)
                    .WhereIf(input.IsPowered.HasValue, x => x.IsPowered == input.IsPowered.Value);

            return query;
        }

        private IQueryable<VehicleCategoryDto> GetVehicleCategoryDtoQuery(IQueryable<VehicleCategory> query) =>
            query.Select(x => new VehicleCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                AssetType = x.AssetType,
                AssetTypeName = x.AssetType.ToString(),
                IsPowered = x.IsPowered,
                SortOrder = x.SortOrder,
            });

        #endregion private methods
    }
}
