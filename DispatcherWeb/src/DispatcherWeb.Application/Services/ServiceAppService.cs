using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes;
using DispatcherWeb.Services.Dto;
using DispatcherWeb.Services.Exporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Services
{
    [AbpAuthorize]
    public class ServiceAppService : DispatcherWebAppServiceBase, IServiceAppService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IRepository<OfficeServicePrice> _servicePriceRepository;
        private readonly IRepository<QuoteService> _quoteServiceRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<ProjectService> _projectServiceRepository;
        private readonly IServiceListCsvExporter _serviceListCsvExporter;

        public ServiceAppService(
            IServiceRepository serviceRepository,
            IRepository<OfficeServicePrice> servicePriceRepository,
            IRepository<QuoteService> quoteServiceRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<ProjectService> projectServiceRepository,
            IServiceListCsvExporter serviceListCsvExporter
            )
        {
            _serviceRepository = serviceRepository;
            _servicePriceRepository = servicePriceRepository;
            _quoteServiceRepository = quoteServiceRepository;
            _orderLineRepository = orderLineRepository;
            _projectServiceRepository = projectServiceRepository;
            _serviceListCsvExporter = serviceListCsvExporter;
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task<PagedResultDto<ServiceDto>> GetServices(GetServicesInput input)
        {
            var query = GetFilteredServiceQuery(input);

            var totalCount = await query.CountAsync();

            var items = await GetServiceDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ServiceDto>(
                totalCount,
                items);
        }

        private IQueryable<Service> GetFilteredServiceQuery(IGetServiceListFilter input) =>
            _serviceRepository.GetAll()
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Service1.StartsWith(input.Name))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive);

        private IQueryable<ServiceDto> GetServiceDtoQuery(IQueryable<Service> query) =>
            query.Select(x => new ServiceDto
            {
                Id = x.Id,
                Service1 = x.Service1,
                Description = x.Description,
                IsActive = x.IsActive,
                DisallowDataMerge = false
            });

        [AbpAuthorize(AppPermissions.Pages_Services)]
        [HttpPost]
        public async Task<FileDto> GetServicesToCsv(GetServicesInput input)
        {
            var query = GetFilteredServiceQuery(input);
            var items = await GetServiceDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return _serviceListCsvExporter.ExportToFile(items);

        }

        public async Task<PagedResultDto<SelectListDto>> GetServicesSelectList(GetSelectListInput input) =>
            await GetServicesSelectList(input, true);
        public async Task<PagedResultDto<SelectListDto>> GetAllServicesSelectList(GetSelectListInput input) =>
            await GetServicesSelectList(input, false);
        private async Task<PagedResultDto<SelectListDto>> GetServicesSelectList(GetSelectListInput input, bool showOnlyActive)
        {
            var query = _serviceRepository.GetAll()
                .WhereIf(showOnlyActive, x => x.IsActive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Service1
                });

            return await query.GetSelectListResult(input);
        }

        public async Task<PagedResultDto<SelectListDto>> GetServicesWithTaxInfoSelectList(GetSelectListInput input)
        {
            var query = _serviceRepository.GetAll()
                .Where(x => x.IsActive)
                .Select(x => new SelectListDto<ServiceSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Service1,
                    Item = new ServiceSelectListInfoDto
                    {
                        IsTaxable = x.IsTaxable
                    }
                });

            return await query.GetSelectListResult(input);
        }

        [HttpPost]
        public async Task<ListResultDto<SelectListDto>> GetServicesByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = await _serviceRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Service1
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        public async Task<ServicePricingDto> GetServicePricing(GetServicePricingInput input)
        {
            var servicePrices = await _servicePriceRepository.GetAll()
                .Where(x => x.ServiceId == input.ServiceId
                            && (x.MaterialUomId == input.MaterialUomId || x.FreightUomId == input.FreightUomId)
                            && x.OfficeId == OfficeId)
                .Select(x => new
                {
                    HasPricing = true,
                    x.PricePerUnit,
                    x.FreightRate,
                    x.MaterialUomId,
                    x.FreightUomId
                })
                .ToListAsync();

            var serviceMatchingBoth = servicePrices.FirstOrDefault(x => x.MaterialUomId == input.MaterialUomId && x.FreightUomId == input.FreightUomId);
            var serviceMatchingMaterial = servicePrices.FirstOrDefault(x => x.MaterialUomId == input.MaterialUomId);
            var serviceMatchingFreight = servicePrices.FirstOrDefault(x => x.FreightUomId == input.FreightUomId);

            ServicePricingDto servicePricing = servicePrices.Any()
                ? new ServicePricingDto
                {
                    HasPricing = true,
                    PricePerUnit = serviceMatchingBoth?.PricePerUnit ?? serviceMatchingMaterial?.PricePerUnit,
                    FreightRate = serviceMatchingBoth?.FreightRate ?? serviceMatchingFreight?.FreightRate,
                } : new ServicePricingDto();

            if (input.QuoteServiceId.HasValue)
            {
                var quotePricing = await _quoteServiceRepository.GetAll()
                    .Where(x => x.Id == input.QuoteServiceId)
                    .Select(x => new
                    {
                        x.PricePerUnit,
                        x.FreightRate,
                        x.MaterialUomId,
                        x.FreightUomId
                    })
                    .FirstOrDefaultAsync();

                if (quotePricing != null)
                {
                    servicePricing.QuoteBasedPricing = new QuoteServicePricingDto
                    {
                        PricePerUnit = quotePricing.PricePerUnit,
                        FreightRate = quotePricing.FreightRate
                    };
                }
            }

            return servicePricing;
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task<ServiceEditDto> GetServiceForEdit(NullableIdNameDto input)
        {
            ServiceEditDto serviceEditDto;

            if (input.Id.HasValue)
            {
                serviceEditDto = await _serviceRepository.GetAll()
                    .Where(x => x.Id == input.Id)
                    .Select(x => new ServiceEditDto
                    {
                        Id = x.Id,
                        Service1 = x.Service1,
                        Description = x.Description,
                        IsActive = x.IsActive,
                        Type = x.Type,
                        IsTaxable = x.IsTaxable,
                        IncomeAccount = x.IncomeAccount
                    })
                    .FirstAsync();
            }
            else
            {
                serviceEditDto = new ServiceEditDto
                {
                    IsActive = true,
                    Service1 = input.Name
                };
            }

            return serviceEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task<ServiceEditDto> EditService(ServiceEditDto model)
        {
            var service = model.Id.HasValue ? await _serviceRepository.GetAsync(model.Id.Value) : new Service();

            service.Service1 = model.Service1;
            service.Description = model.Description;
            service.IsActive = model.IsActive;
            service.Type = model.Type;
            service.IsTaxable = model.IsTaxable;
            service.IncomeAccount = model.IncomeAccount;

            if (!model.Id.HasValue)
            {
                model.Id = await _serviceRepository.InsertAndGetIdAsync(service);
            }

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task<bool> CanDeleteService(EntityDto input)
        {
            if (await _orderLineRepository.GetAll().AnyAsync(x => x.ServiceId == input.Id)
                || await _quoteServiceRepository.GetAll().AnyAsync(x => x.ServiceId == input.Id)
                || await _projectServiceRepository.GetAll().AnyAsync(x => x.ServiceId == input.Id))
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task DeleteService(EntityDto input)
        {
            var canDelete = await CanDeleteService(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _serviceRepository.DeleteAsync(input.Id);
        }

        //*************************************************//

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task<PagedResultDto<ServicePriceDto>> GetServicePrices(GetServicePricesInput input)
        {
            var query = _servicePriceRepository.GetAll()
                .Where(x => x.ServiceId == input.ServiceId && x.OfficeId == OfficeId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new ServicePriceDto
                {
                    Id = x.Id,
                    ServiceId = x.ServiceId,
                    OfficeId = x.OfficeId,
                    PricePerUnit = x.PricePerUnit,
                    FreightRate = x.FreightRate,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomName = x.FreightUom.Name,
                    Designation = x.Designation
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<ServicePriceDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task<ServicePriceEditDto> GetServicePriceForEdit(NullableIdDto input)
        {
            ServicePriceEditDto servicePriceEditDto;

            if (input.Id.HasValue)
            {
                servicePriceEditDto = await _servicePriceRepository.GetAll()
                    .Where(x => x.Id == input.Id)
                    .Select(x => new ServicePriceEditDto
                    {
                        Id = x.Id,
                        ServiceId = x.ServiceId,
                        OfficeId = x.OfficeId,
                        PricePerUnit = x.PricePerUnit,
                        FreightRate = x.FreightRate,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation
                    })
                    .FirstAsync();
            }
            else
            {
                servicePriceEditDto = new ServicePriceEditDto
                {
                    OfficeId = OfficeId
                };
            }

            return servicePriceEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task EditServicePrice(ServicePriceEditDto model)
        {
            await _servicePriceRepository.InsertOrUpdateAndGetIdAsync(new OfficeServicePrice
            {
                Id = model.Id ?? 0,
                ServiceId = model.ServiceId,
                OfficeId = OfficeId,
                PricePerUnit = model.PricePerUnit,
                FreightRate = model.FreightRate,
                MaterialUomId = model.MaterialUomId,
                FreightUomId = model.FreightUomId,
                Designation = model.Designation,
                TenantId = Session.TenantId ?? 0
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Services)]
        public async Task DeleteServicePrice(EntityDto input)
        {
            await _servicePriceRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Services_Merge)]
        public async Task MergeServices(DataMergeInput input)
        {
            await _serviceRepository.MergeServicesAsync(input.IdsToMerge, input.MainRecordId, AbpSession.TenantId);
        }
    }
}
