using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Security;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Offices.Dto;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Offices
{
    [AbpAuthorize]
    public class OfficeAppService : DispatcherWebAppServiceBase, IOfficeAppService
    {
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ISingleOfficeAppService _singleOfficeService;

        public OfficeAppService(
            IRepository<Office> officeRepository,
            IRepository<Order> orderRepository,
            ISingleOfficeAppService singleOfficeService
            )
        {
            _officeRepository = officeRepository;
            _orderRepository = orderRepository;
            _singleOfficeService = singleOfficeService;
        }

        public async Task<ListResultDto<OfficeDto>> GetAllOffices()
        {
            var items = await _officeRepository.GetAll()
                .Select(x => new OfficeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TruckColor = x.TruckColor
                })
                .ToListAsync();

            return new ListResultDto<OfficeDto>(items);
        }

        public async Task<ListResultDto<SelectListDto>> GetAllOfficesSelectList()
        {
            var items = await _officeRepository.GetAll()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                })
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<PagedResultDto<OfficeDto>> GetOffices(GetOfficesInput input)
        {
            var query = _officeRepository.GetAll();

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new OfficeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TruckColor = x.TruckColor
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OfficeDto>(
                totalCount,
                items);
        }

        public async Task<PagedResultDto<SelectListDto>> GetOfficesSelectList(GetSelectListInput input)
        {
            var query = _officeRepository.GetAll()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                });

            return await query.GetSelectListResult(input);
        }

        public async Task<OfficeEditDto> GetOfficeForEdit(NullableIdDto input)
        {
            OfficeEditDto officeEditDto;

            if (input.Id.HasValue)
            {
                var office = await _officeRepository.GetAsync(input.Id.Value);
                officeEditDto = new OfficeEditDto
                {
                    Id = office.Id,
                    Name = office.Name,
                    TruckColor = office.TruckColor,
                    CopyChargeTo = office.CopyDeliverToLoadAtChargeTo,
                    HeartlandPublicKey = office.HeartlandPublicKey,
                    HeartlandSecretKey = SimpleStringCipher.Instance.Decrypt(office.HeartlandSecretKey),
                    FuelIds = office.FuelIds,
                    DefaultStartTime = office.DefaultStartTime
                };

                if (string.IsNullOrEmpty(officeEditDto.HeartlandPublicKey))
                {
                    officeEditDto.HeartlandPublicKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);
                }

                if (string.IsNullOrEmpty(officeEditDto.HeartlandSecretKey))
                {
                    officeEditDto.HeartlandSecretKey = SimpleStringCipher.Instance.Decrypt(await SettingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey));
                }

                officeEditDto.HeartlandSecretKey = officeEditDto.HeartlandSecretKey.IsNullOrEmpty() ? string.Empty : DispatcherWebConsts.PasswordHasntBeenChanged;
            }
            else
            {
                officeEditDto = new OfficeEditDto
                {
                    DefaultStartTime = await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.DispatchingAndMessaging.DefaultStartTime)
                };
            }

            officeEditDto.DefaultStartTime = officeEditDto.DefaultStartTime?.ConvertTimeZoneTo(await GetTimezone());

            return officeEditDto;
        }

        public async Task EditOffice(OfficeEditDto model)
        {
            if (!model.Id.HasValue)
            {
                await CheckAllowMultiOffice();
                await _singleOfficeService.Reset();
            }

            var entity = model.Id.HasValue ? await _officeRepository.GetAsync(model.Id.Value) : new Office();

            entity.Name = model.Name;
            entity.TruckColor = model.TruckColor;
            entity.CopyDeliverToLoadAtChargeTo = model.CopyChargeTo;
            entity.TenantId = Session.TenantId ?? 0;
            entity.FuelIds = model.FuelIds;
            entity.DefaultStartTime = model.DefaultStartTime?.ConvertTimeZoneFrom(await GetTimezone());

            if (model.HeartlandSecretKey != DispatcherWebConsts.PasswordHasntBeenChanged)
            {
                var tenantHeartlandSecretKey = SimpleStringCipher.Instance.Decrypt(await SettingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey));

                if (model.HeartlandSecretKey == tenantHeartlandSecretKey)
                {
                    model.HeartlandSecretKey = null;
                }

                entity.HeartlandSecretKey = SimpleStringCipher.Instance.Encrypt(model.HeartlandSecretKey);
            }

            var tenantHeartlandPublicKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);

            if (model.HeartlandPublicKey == tenantHeartlandPublicKey)
            {
                model.HeartlandPublicKey = null;
            }

            entity.HeartlandPublicKey = model.HeartlandPublicKey;

            if (await _officeRepository.GetAll()
                .WhereIf(model.Id != 0, x => x.Id != model.Id)
                .AnyAsync(x => x.Name == model.Name)
            )
            {
                throw new UserFriendlyException($"Office with name '{model.Name}' already exists!");
            }

            await _officeRepository.InsertOrUpdateAndGetIdAsync(entity);
        }

        private async Task CheckAllowMultiOffice()
        {
            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature))
            {
                return;
            }
            int currentOfficesNumber = await _officeRepository.CountAsync();
            if (currentOfficesNumber > 0)
            {
                throw new AbpAuthorizationException("You cannot have more than one office.");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<int> GetOfficesNumber()
        {
            return await _officeRepository.CountAsync();
        }

        public async Task<bool> CanDeleteOffice(EntityDto input)
        {
            var record = await _officeRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasTrucks = x.Trucks.Any(),
                    HasUsers = x.Users.Any()
                })
                .SingleAsync();

            if (record.HasTrucks || record.HasUsers)
            {
                return false;
            }

            var hasOrders = await _orderRepository.GetAll().Where(x => x.LocationId == input.Id || x.SharedOrders.Any(s => s.OfficeId == input.Id)).AnyAsync();
            if (hasOrders)
            {
                return false;
            }

            return true;
        }

        public async Task DeleteOffice(EntityDto input)
        {
            var canDelete = await CanDeleteOffice(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _officeRepository.DeleteAsync(input.Id);
        }
    }
}
