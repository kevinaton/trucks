using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Locations.Exporting;
using DispatcherWeb.Orders;
using DispatcherWeb.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Locations
{
    [AbpAuthorize]
    public class LocationAppService : DispatcherWebAppServiceBase, ILocationAppService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IRepository<SupplierContact> _supplierContactRepository;
        private readonly IRepository<LocationCategory> _locationCategoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<ProjectService> _projectServiceRepository;
        private readonly ILocationListCsvExporting _locationListCsvExporting;

        public LocationAppService(
            ILocationRepository locationRepository,
            IRepository<SupplierContact> supplierContactRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<ProjectService> projectServiceRepository,
            ILocationListCsvExporting locationListCsvExporting
            )
        {
            _locationRepository = locationRepository;
            _supplierContactRepository = supplierContactRepository;
            _locationCategoryRepository = locationCategoryRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _projectServiceRepository = projectServiceRepository;
            _locationListCsvExporting = locationListCsvExporting;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<PagedResultDto<LocationDto>> GetLocations(GetLocationsInput input)
        {
            var query = GetFilteredLocationQuery(input);

            var totalCount = await query.CountAsync();

            var items = await GetLocationDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<LocationDto>(
                totalCount,
                items);
        }

        private IQueryable<Location> GetFilteredLocationQuery(IGetLocationFilteredList input) =>
            _locationRepository.GetAll()
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.StartsWith(input.Name) || x.Abbreviation.StartsWith(input.Name))
                .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
                .WhereIf(!input.City.IsNullOrEmpty(), x => x.City.StartsWith(input.City))
                .WhereIf(!input.State.IsNullOrEmpty(), x => x.State.StartsWith(input.State))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .WhereIf(input.WithCoordinates, x => x.Latitude != null && x.Longitude != null);

        private IQueryable<LocationDto> GetLocationDtoQuery(IQueryable<Location> query) =>
            query.Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryName = x.Category.Name,
                StreetAddress = x.StreetAddress,
                City = x.City,
                State = x.State,
                ZipCode = x.ZipCode,
                CountryCode = x.CountryCode,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                IsActive = x.IsActive,
                PredefinedLocationKind = x.PredefinedLocationKind,
                DisallowDataMerge = x.PredefinedLocationKind != null,
                Abbreviation = x.Abbreviation,
                Notes = x.Notes
            });

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        [HttpPost]
        public async Task<FileDto> GetLocationsToCsv(GetLocationsInput input)
        {
            var query = GetFilteredLocationQuery(input);
            var items = await GetLocationDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return _locationListCsvExporting.ExportToFile(items);
        }

        public async Task<PagedResultDto<SelectListDto>> GetLocationsSelectList(GetSelectListInput input) =>
            await GetLocationsSelectList(input, true);
        public async Task<PagedResultDto<SelectListDto>> GetAllLocationsSelectList(GetSelectListInput input) =>
            await GetLocationsSelectList(input, false);
        private async Task<PagedResultDto<SelectListDto>> GetLocationsSelectList(GetSelectListInput input, bool showOnlyActive)
        {
            var query = _locationRepository.GetAll()
                .WhereIf(showOnlyActive, x => x.IsActive)
                .ToSelectListDto();

            return await query.GetSelectListResult(input);
        }

        [HttpPost]
        public async Task<ListResultDto<SelectListDto>> GetLocationsByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = (await _locationRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .ToSelectListDto()
                .OrderBy(x => x.Name)
                .ToListAsync())
                .Select(AppServiceExtensions.ConvertLocationSelectListResult)
                .ToList();

            return new ListResultDto<SelectListDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<LocationEditDto> GetLocationForEdit(GetLocationForEditInput input)
        {
            LocationEditDto locationEditDto;

            if (input.Id.HasValue)
            {
                locationEditDto = await _locationRepository.GetAll()
                    .Select(l => new LocationEditDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        CategoryId = l.CategoryId,
                        CategoryName = l.Category.Name,
                        IsTemporary = l.Category.Name == "Temporary",
                        StreetAddress = l.StreetAddress,
                        City = l.City,
                        State = l.State,
                        ZipCode = l.ZipCode,
                        CountryCode = l.CountryCode,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        PlaceId = l.PlaceId,
                        IsActive = l.IsActive,
                        Abbreviation = l.Abbreviation,
                        Notes = l.Notes

                    })
                    .SingleAsync(s => s.Id == input.Id.Value);
            }
            else
            {
                locationEditDto = new LocationEditDto
                {
                    IsActive = true,
                    Name = input.Name,
                };

                if (input.Temporary)
                {
                    var temporaryCategory = await _locationCategoryRepository.GetAll()
                        .Where(x => x.Name == "Temporary")
                        .FirstOrDefaultAsync();

                    locationEditDto.CategoryId = temporaryCategory?.Id;
                    locationEditDto.CategoryName = temporaryCategory?.Name;
                    locationEditDto.IsTemporary = true;
                }

                locationEditDto.MergeWithDuplicateSilently = input.MergeWithDuplicateSilently;
            }

            return locationEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<LocationEditDto> EditLocation(LocationEditDto model)
        {
            var location = model.Id.HasValue ? await _locationRepository.GetAsync(model.Id.Value) : new Location();

            if (location.PredefinedLocationKind != null)
            {
                return model;
            }

            location.Name = model.Name;
            location.CategoryId = model.CategoryId;
            location.StreetAddress = model.StreetAddress;
            location.City = model.City;
            location.State = model.State;
            location.ZipCode = model.ZipCode;
            location.CountryCode = model.CountryCode;
            location.Latitude = model.Latitude;
            location.Longitude = model.Longitude;
            location.PlaceId = model.PlaceId;
            location.IsActive = model.IsActive;
            location.Abbreviation = model.Abbreviation;
            location.Notes = model.Notes;

            if (model.Id.HasValue)
            {
                return model;
            }
            else
            {
                model.Id = await _locationRepository.InsertAndGetIdAsync(location);
                return model;
            }
        }

        public async Task<LocationEditDto> CreateOrGetExistingLocation(CreateOrGetExistingLocationInput model)
        {
            var category = await _locationCategoryRepository.GetAll()
                .Where(x => x.PredefinedLocationCategoryKind == model.PredefinedLocationCategoryKind)
                .Select(x => new
                {
                    x.Id,
                    x.Name
                }).FirstOrDefaultAsync();

            if (category == null)
            {
                Logger.Error("PredefinedLocationCategory wasn't found: " + model.PredefinedLocationCategoryKind?.ToIntString());
                throw new UserFriendlyException("Predefined Location Category wasn't found");
            }

            var duplicate = await FindExistingLocationDuplicate(new LocationEditDto
            {
                CategoryId = category.Id,
                Name = model.Name,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                PlaceId = model.PlaceId,
                StreetAddress = model.StreetAddress,
                City = model.City,
                State = model.State
            });

            if (duplicate != null)
            {
                return duplicate;
            }

            var result = await EditLocation(new LocationEditDto
            {
                Name = model.Name?.WithMaxLength(100),
                CategoryId = category.Id,
                CategoryName = category.Name,
                StreetAddress = model.StreetAddress?.WithMaxLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength),
                City = model.City?.WithMaxLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength),
                State = model.State?.WithMaxLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength),
                ZipCode = model.ZipCode?.WithMaxLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength),
                CountryCode = model.CountryCode?.WithMaxLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength),
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                PlaceId = model.PlaceId,
                IsActive = model.IsActive,
                Abbreviation = model.Abbreviation?.WithMaxLength(10),
                Notes = model.Notes?.WithMaxLength(1000)
            });

            return result;
        }

        public async Task<LocationEditDto> FindExistingLocationDuplicate(LocationEditDto model)
        {
            //(place id is specified and matches an existing record) 
            //or (address and coordinates are specified and match) 
            //or (name and address and coordinates are specified and (address matches and (name matches or geocoordinates match)))
            var addressIsSpecified = !string.IsNullOrEmpty(model.StreetAddress) || !string.IsNullOrEmpty(model.City) || !string.IsNullOrEmpty(model.State);
            var coordinatesAreSpecified = model.Latitude != null && model.Longitude != null;
            var result = await _locationRepository.GetAll()
                    .Where(x =>
                        !string.IsNullOrEmpty(model.PlaceId) && x.PlaceId == model.PlaceId
                        || addressIsSpecified && coordinatesAreSpecified && x.StreetAddress == model.StreetAddress && x.City == model.City && x.State == model.State && x.Latitude == model.Latitude && x.Longitude == model.Longitude
                        || (!string.IsNullOrEmpty(model.Name) && addressIsSpecified && coordinatesAreSpecified
                            && x.StreetAddress == model.StreetAddress && x.City == model.City && x.State == model.State
                            && (x.Name == model.Name || x.Latitude == model.Latitude && x.Longitude == x.Longitude)
                            )
                        || !coordinatesAreSpecified && x.Name == model.Name && x.StreetAddress == model.StreetAddress && x.City == model.City && x.State == model.State
                        )
                    .Select(l => new LocationEditDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        CategoryId = l.CategoryId,
                        CategoryName = l.Category.Name,
                        IsTemporary = l.Category.Name == "Temporary",
                        StreetAddress = l.StreetAddress,
                        City = l.City,
                        State = l.State,
                        ZipCode = l.ZipCode,
                        CountryCode = l.CountryCode,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        PlaceId = l.PlaceId,
                        IsActive = l.IsActive,
                        Abbreviation = l.Abbreviation,
                        Notes = l.Notes

                    }).FirstOrDefaultAsync();

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<bool> CanDeleteLocation(EntityDto input)
        {
            var isPredefined = await _locationRepository.GetAll().AnyAsync(x => x.Id == input.Id && x.PredefinedLocationKind != null);
            if (isPredefined)
            {
                return false;
            }

            var hasOrderLines = await _orderLineRepository.GetAll().Where(x => x.LoadAtId == input.Id || x.DeliverToId == input.Id).AnyAsync();
            if (hasOrderLines)
            {
                return false;
            }

            var hasProjectServices = await _projectServiceRepository.GetAll().Where(x => x.LoadAtId == input.Id || x.DeliverToId == input.Id).AnyAsync();
            if (hasProjectServices)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task DeleteLocation(EntityDto input)
        {
            var canDelete = await CanDeleteLocation(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _locationRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<PagedResultDto<SupplierContactDto>> GetSupplierContacts(GetSupplierContactsInput input)
        {
            var query = _supplierContactRepository.GetAll()
                .Where(x => x.LocationId == input.LocationId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new SupplierContactDto
                {
                    Id = x.Id,
                    LocationId = x.LocationId,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    Title = x.Title
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<SupplierContactDto>(
                totalCount,
                items);
        }

        [HttpPost]
        public async Task<ListResultDto<SelectListDto>> GetSupplierContactsByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = await _supplierContactRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<SupplierContactEditDto> GetSupplierContactForEdit(NullableIdDto input)
        {
            SupplierContactEditDto supplierContactEditDto;

            if (input.Id.HasValue)
            {
                var supplierContact = await _supplierContactRepository.GetAsync(input.Id.Value);
                supplierContactEditDto = new SupplierContactEditDto
                {
                    Id = supplierContact.Id,
                    LocationId = supplierContact.LocationId,
                    Name = supplierContact.Name,
                    Phone = supplierContact.Phone,
                    Email = supplierContact.Email,
                    Title = supplierContact.Title
                };
            }
            else
            {
                supplierContactEditDto = new SupplierContactEditDto();
            }

            return supplierContactEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task EditSupplierContact(SupplierContactEditDto model)
        {
            await _supplierContactRepository.InsertOrUpdateAndGetIdAsync(new SupplierContact
            {
                Id = model.Id ?? 0,
                LocationId = model.LocationId,
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Title = model.Title,
                TenantId = Session.TenantId ?? 0
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task DeleteSupplierContact(EntityDto input)
        {
            await _supplierContactRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations_Merge)]
        public async Task MergeLocations(DataMergeInput input)
        {
            await _locationRepository.MergeLocationsAsync(input.IdsToMerge, input.MainRecordId, AbpSession.TenantId);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations_Merge)]
        public async Task MergeSupplierContacts(DataMergeInput input)
        {
            await _locationRepository.MergeSupplierContactsAsync(input.IdsToMerge, input.MainRecordId, AbpSession.TenantId);
        }

        public async Task<PagedResultDto<SelectListDto>> GetLocationCategorySelectList(GetSelectListInput input)
        {
            var query = _locationCategoryRepository.GetAll()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                });

            return await query.GetSelectListResult(input);
        }
    }

}
