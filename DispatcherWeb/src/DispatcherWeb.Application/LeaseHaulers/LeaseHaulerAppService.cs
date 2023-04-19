using System;
using System.Linq;
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
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.LeaseHaulers.Exporting;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Security;
using DispatcherWeb.Trucks;
using DispatcherWeb.Url;
using DispatcherWeb.VehicleCategories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHauler)]
    public class LeaseHaulerAppService : DispatcherWebAppServiceBase, ILeaseHaulerAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IRepository<LeaseHaulerContact> _leaseHaulerContactRepository;
        private readonly IRepository<LeaseHaulerTruck> _leaseHaulerTruckRepository;
        private readonly IRepository<LeaseHaulerDriver> _leaseHaulerDriverRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly ILeaseHaulerListCsvExporter _leaseHaulerListCsvExporter;
        private readonly IDriverAppService _driverAppService;
        private readonly ITruckAppService _truckAppService;
        private readonly IUserCreatorService _userCreatorService;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly IDriverInactivatorService _driverInactivatorService;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserEmailer _userEmailer;
        private readonly IPasswordComplexitySettingStore _passwordComplexitySettingStore;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;

        public LeaseHaulerAppService(
            IRepository<LeaseHauler> leaseHaulerRepository,
            IRepository<LeaseHaulerContact> leaseHaulerContactRepository,
            IRepository<LeaseHaulerTruck> leaseHaulerTruckRepository,
            IRepository<LeaseHaulerDriver> leaseHaulerDriverRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Truck> truckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            ILeaseHaulerListCsvExporter leaseHaulerListCsvExporter,
            IDriverAppService driverAppService,
            ITruckAppService truckAppService,
            IUserCreatorService userCreatorService,
            IDriverUserLinkService driverUserLinkService,
            IDriverInactivatorService driverInactivatorService,
            ISingleOfficeAppService singleOfficeService,
            IPasswordHasher<User> passwordHasher,
            IUserEmailer userEmailer,
            IPasswordComplexitySettingStore passwordComplexitySettingStore,
            ICrossTenantOrderSender crossTenantOrderSender
            )
        {
            _leaseHaulerRepository = leaseHaulerRepository;
            _leaseHaulerContactRepository = leaseHaulerContactRepository;
            _leaseHaulerTruckRepository = leaseHaulerTruckRepository;
            _leaseHaulerDriverRepository = leaseHaulerDriverRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _leaseHaulerListCsvExporter = leaseHaulerListCsvExporter;
            _driverAppService = driverAppService;
            _truckAppService = truckAppService;
            _userCreatorService = userCreatorService;
            _driverUserLinkService = driverUserLinkService;
            _driverInactivatorService = driverInactivatorService;
            _singleOfficeService = singleOfficeService;
            _passwordHasher = passwordHasher;
            _userEmailer = userEmailer;
            _passwordComplexitySettingStore = passwordComplexitySettingStore;
            _crossTenantOrderSender = crossTenantOrderSender;
            AppUrlService = NullAppUrlService.Instance;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PagedResultDto<LeaseHaulerDto>> GetLeaseHaulers(GetLeaseHaulersInput input)
        {
            var query = GetFilteredLeaseHaulerQuery(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LeaseHaulerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    City = x.City,
                    State = x.State,
                    ZipCode = x.ZipCode,
                    CountryCode = x.CountryCode,
                    AccountNumber = x.AccountNumber,
                    StreetAddress1 = x.StreetAddress1,
                    PhoneNumber = x.PhoneNumber,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerDto>(
                totalCount,
                items);
        }

        public IQueryable<LeaseHauler> GetFilteredLeaseHaulerQuery(IGetLeaseHaulerListFilter input) =>
            _leaseHaulerRepository.GetAll()
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.StartsWith(input.Name))
                .WhereIf(!input.City.IsNullOrEmpty(), x => x.City.StartsWith(input.City))
                .WhereIf(!input.State.IsNullOrEmpty(), x => x.State.StartsWith(input.State));

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        [HttpPost]
        public async Task<FileDto> GetLeaseHaulersToCsv(GetLeaseHaulersInput input)
        {
            var query = GetFilteredLeaseHaulerQuery(input);
            var items = await query
                .Select(x => new LeaseHaulerEditDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    StreetAddress1 = x.StreetAddress1,
                    StreetAddress2 = x.StreetAddress2,
                    City = x.City,
                    State = x.State,
                    ZipCode = x.ZipCode,
                    CountryCode = x.CountryCode,
                    AccountNumber = x.AccountNumber,
                    PhoneNumber = x.PhoneNumber,
                    IsActive = x.IsActive,
                    HaulingCompanyTenantId = x.HaulingCompanyTenantId
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            return _leaseHaulerListCsvExporter.ExportToFile(items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler)]
        public async Task<PagedResultDto<SelectListDto>> GetLeaseHaulersSelectList(GetLeaseHaulersSelectListInput input)
        {
            var query = _leaseHaulerRepository.GetAll()
                .WhereIf(!input.IncludeInactive, x => x.IsActive)
                .WhereIf(input.HasHaulingCompanyTenantId == true, x => x.HaulingCompanyTenantId.HasValue)
                .WhereIf(input.HasHaulingCompanyTenantId == false, x => x.HaulingCompanyTenantId == null)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<LeaseHaulerEditDto> GetLeaseHaulerForEdit(NullableIdDto input)
        {
            LeaseHaulerEditDto leaseHaulerEditDto;

            if (input.Id.HasValue)
            {
                var leaseHauler = await _leaseHaulerRepository.GetAsync(input.Id.Value);
                leaseHaulerEditDto = new LeaseHaulerEditDto
                {
                    Id = leaseHauler.Id,
                    Name = leaseHauler.Name,
                    StreetAddress1 = leaseHauler.StreetAddress1,
                    StreetAddress2 = leaseHauler.StreetAddress2,
                    City = leaseHauler.City,
                    State = leaseHauler.State,
                    ZipCode = leaseHauler.ZipCode,
                    CountryCode = leaseHauler.CountryCode,
                    AccountNumber = leaseHauler.AccountNumber,
                    PhoneNumber = leaseHauler.PhoneNumber,
                    IsActive = leaseHauler.IsActive,
                    HaulingCompanyTenantId = leaseHauler.HaulingCompanyTenantId
                };
            }
            else
            {
                leaseHaulerEditDto = new LeaseHaulerEditDto
                {
                    IsActive = true
                };
            }

            return leaseHaulerEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<int> EditLeaseHauler(LeaseHaulerEditDto model)
        {
            var leaseHauler = model.Id.HasValue ? await _leaseHaulerRepository.GetAsync(model.Id.Value) : new LeaseHauler();

            if (await _leaseHaulerRepository.GetAll().AnyAsync(lh => lh.Id != leaseHauler.Id && lh.Name == model.Name))
            {
                throw new UserFriendlyException($"A Lease Hauler with name '{model.Name}' already exists!");
            }

            leaseHauler.Name = model.Name;
            leaseHauler.StreetAddress1 = model.StreetAddress1;
            leaseHauler.StreetAddress2 = model.StreetAddress2;
            leaseHauler.City = model.City;
            leaseHauler.State = model.State;
            leaseHauler.ZipCode = model.ZipCode;
            leaseHauler.CountryCode = model.CountryCode;
            leaseHauler.AccountNumber = model.AccountNumber;
            leaseHauler.PhoneNumber = model.PhoneNumber;
            leaseHauler.IsActive = model.IsActive;

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowSendingOrdersToDifferentTenant)
                && await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_SetHaulingCompanyTenantId))
            {
                if (model.HaulingCompanyTenantId.HasValue)
                {
                    if (model.HaulingCompanyTenantId == AbpSession.TenantId)
                    {
                        throw new UserFriendlyException($"{model.HaulingCompanyTenantId} is your own tenant id");
                    }

                    if (!await TenantManager.Tenants.AnyAsync(x => x.Id == model.HaulingCompanyTenantId))
                    {
                        throw new UserFriendlyException($"Tenant with id {model.HaulingCompanyTenantId} wasn't found");
                    }
                }
                leaseHauler.HaulingCompanyTenantId = model.HaulingCompanyTenantId;
            }

            if (model.Id.HasValue)
            {
                return model.Id.Value;
            }
            else
            {
                return await _leaseHaulerRepository.InsertAndGetIdAsync(leaseHauler);
            }
        }

        //*************************************************//

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler)]
        public async Task<PagedResultDto<LeaseHaulerContactDto>> GetLeaseHaulerContacts(GetLeaseHaulerContactsInput input)
        {
            var query = _leaseHaulerContactRepository.GetAll()
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LeaseHaulerContactDto
                {
                    Id = x.Id,
                    LeaseHaulerId = x.LeaseHaulerId,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    CellPhoneNumber = x.CellPhoneNumber,
                    Title = x.Title
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerContactDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler)]
        public async Task<PagedResultDto<LeaseHaulerTruckDto>> GetLeaseHaulerTrucks(GetLeaseHaulerTrucksInput input)
        {
            var query = _leaseHaulerTruckRepository.GetAll()
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => x.Truck)
                .Select(x => new LeaseHaulerTruckDto
                {
                    Id = x.Id,
                    TruckCode = x.TruckCode,
                    VehicleCategoryName = x.VehicleCategory.Name,
                    DefaultDriverName = x.DefaultDriver.FirstName + " " + x.DefaultDriver.LastName,
                    IsActive = x.IsActive,
                    AlwaysShowOnSchedule = x.LeaseHaulerTruck.AlwaysShowOnSchedule
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerTruckDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler)]
        public async Task<PagedResultDto<LeaseHaulerDriverDto>> GetLeaseHaulerDrivers(GetLeaseHaulerDriversInput input)
        {
            var query = _leaseHaulerDriverRepository.GetAll()
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => x.Driver)
                .Select(x => new LeaseHaulerDriverDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    IsInactive = x.IsInactive
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerDriverDto>(
                totalCount,
                items);
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<SelectListDto>> GetLeaseHaulerDriversSelectList(GetLeaseHaulerDriversSelectListInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                return await _leaseHaulerDriverRepository.GetAll()
                    .WhereIf(input.LeaseHaulerId.HasValue, x => x.LeaseHaulerId == input.LeaseHaulerId)
                    .WhereIf(!input.LeaseHaulerId.HasValue, x => x.TenantId == Session.TenantId)
                    .Select(x => x.Driver)
                    .SelectIdName()
                    .GetSelectListResult(input);
            }
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<SelectListDto>> GetLeaseHaulerTrucksSelectList(GetLeaseHaulerTrucksSelectListInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                return await _leaseHaulerTruckRepository.GetAll()
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId)
                .Select(x => x.Truck)
                .Where(x => x.IsActive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.TruckCode
                })
                .GetSelectListResult(input);
            }
        }

        public async Task<ListResultDto<SelectListDto>> GetContactsForLeaseHauler(NullableIdDto input)
        {
            if (input.Id == null)
            {
                return new ListResultDto<SelectListDto>();
            }
            var contacts = await _leaseHaulerContactRepository.GetAll()
                .Where(x => x.LeaseHaulerId == input.Id)
                .OrderBy(x => x.Id)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                })
                .ToListAsync();
            return new ListResultDto<SelectListDto>(contacts);
        }

        public async Task<ListResultDto<LeaseHaulerContactSelectListDto>> GetLeaseHaulerContactSelectList(int leaseHaulerId, int? leaseHaulerContactId, LeaseHaulerMessageType messageType)
        {
            var contacts = await _leaseHaulerContactRepository.GetAll()
                .Where(lhc => lhc.LeaseHaulerId == leaseHaulerId)
                .WhereIf(messageType == LeaseHaulerMessageType.Sms, lhc => !string.IsNullOrEmpty(lhc.CellPhoneNumber))
                .WhereIf(messageType == LeaseHaulerMessageType.Email, lhc => !string.IsNullOrEmpty(lhc.Email))
                .WhereIf(leaseHaulerContactId.HasValue, lhc => lhc.Id == leaseHaulerContactId)
                .OrderBy(lhc => lhc.Name)
                .Select(lhc => new LeaseHaulerContactSelectListDto
                {
                    Id = lhc.Id.ToString(),
                    Name = lhc.Name,
                    IsDefault = lhc.IsDispatcher || leaseHaulerContactId.HasValue,
                })
                .ToListAsync();
            return new ListResultDto<LeaseHaulerContactSelectListDto>(contacts);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<LeaseHaulerContactEditDto> GetLeaseHaulerContactForEdit(NullableIdDto input)
        {
            LeaseHaulerContactEditDto leaseHaulerContactEditDto;

            if (input.Id.HasValue)
            {
                var leaseHaulerContact = await _leaseHaulerContactRepository.GetAsync(input.Id.Value);
                leaseHaulerContactEditDto = new LeaseHaulerContactEditDto
                {
                    Id = leaseHaulerContact.Id,
                    LeaseHaulerId = leaseHaulerContact.LeaseHaulerId,
                    Name = leaseHaulerContact.Name,
                    Phone = leaseHaulerContact.Phone,
                    Email = leaseHaulerContact.Email,
                    Title = leaseHaulerContact.Title,
                    CellPhoneNumber = leaseHaulerContact.CellPhoneNumber,
                    NotifyPreferredFormat = leaseHaulerContact.NotifyPreferredFormat,
                    IsDispatcher = leaseHaulerContact.IsDispatcher,
                };
            }
            else
            {
                leaseHaulerContactEditDto = new LeaseHaulerContactEditDto();
            }

            return leaseHaulerContactEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<LeaseHaulerTruckEditDto> GetLeaseHaulerTruckForEdit(NullableIdDto input)
        {
            LeaseHaulerTruckEditDto leaseHaulerTruckEditDto;

            if (input.Id.HasValue)
            {
                leaseHaulerTruckEditDto = await _truckRepository.GetAll()
                    .Select(t => new LeaseHaulerTruckEditDto
                    {
                        Id = t.Id,
                        TruckCode = t.TruckCode,
                        LeaseHaulerId = t.LeaseHaulerTruck.LeaseHaulerId,
                        VehicleCategoryId = t.VehicleCategoryId,
                        VehicleCategoryName = t.VehicleCategory.Name,
                        DefaultDriverId = t.DefaultDriverId,
                        DefaultDriverName = t.DefaultDriver != null ? t.DefaultDriver.FirstName + " " + t.DefaultDriver.LastName : "",
                        IsActive = t.IsActive,
                        InactivationDate = t.InactivationDate,
                        CanPullTrailer = t.CanPullTrailer,
                        AlwaysShowOnSchedule = t.LeaseHaulerTruck.AlwaysShowOnSchedule,
                        OfficeId = t.LocationId,
                        OfficeName = t.Office.Name,
                        VehicleCategoryIsPowered = t.VehicleCategory.IsPowered,
                        VehicleCategoryAssetType = t.VehicleCategory.AssetType,
                        HaulingCompanyTruckId = t.HaulingCompanyTruckId
                    })
                    .SingleAsync(t => t.Id == input.Id.Value);
            }
            else
            {
                leaseHaulerTruckEditDto = new LeaseHaulerTruckEditDto
                {
                    IsActive = true
                };
            }

            await _singleOfficeService.FillSingleOffice(leaseHaulerTruckEditDto);

            return leaseHaulerTruckEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<LeaseHaulerDriverEditDto> GetLeaseHaulerDriverForEdit(NullableIdDto input)
        {
            LeaseHaulerDriverEditDto leaseHaulerDriverEditDto;

            if (input.Id.HasValue)
            {
                leaseHaulerDriverEditDto = await _driverRepository.GetAll()
                    .Select(x => new LeaseHaulerDriverEditDto
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        DriverIsActive = !x.IsInactive,
                        EmailAddress = x.EmailAddress,
                        CellPhoneNumber = x.CellPhoneNumber,
                        OrderNotifyPreferredFormat = x.OrderNotifyPreferredFormat,
                        UserId = x.UserId,
                        HaulingCompanyDriverId = x.HaulingCompanyDriverId
                    })
                    .SingleAsync(t => t.Id == input.Id.Value);

                var user = await GetUserForLhDriver(leaseHaulerDriverEditDto.EmailAddress, leaseHaulerDriverEditDto.UserId);
                leaseHaulerDriverEditDto.EnableForDriverApplication = user != null && await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);

                leaseHaulerDriverEditDto.SetRandomPassword = user == null;
                leaseHaulerDriverEditDto.ShouldChangePasswordOnNextLogin = user == null;
                leaseHaulerDriverEditDto.SendActivationEmail = user == null;
            }
            else
            {
                leaseHaulerDriverEditDto = new LeaseHaulerDriverEditDto()
                {
                    DriverIsActive = true,
                    SetRandomPassword = true,
                    ShouldChangePasswordOnNextLogin = true,
                    SendActivationEmail = true,
                };
            }

            leaseHaulerDriverEditDto.PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync();

            return leaseHaulerDriverEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task EditLeaseHaulerContact(LeaseHaulerContactEditDto model)
        {
            var leaseHaulerContact = model.Id.HasValue ? await _leaseHaulerContactRepository.GetAsync(model.Id.Value) : new LeaseHaulerContact();

            leaseHaulerContact.LeaseHaulerId = model.LeaseHaulerId;
            leaseHaulerContact.Name = model.Name;
            leaseHaulerContact.Phone = model.Phone;
            leaseHaulerContact.Email = model.Email;
            leaseHaulerContact.Title = model.Title;
            leaseHaulerContact.CellPhoneNumber = model.CellPhoneNumber;
            leaseHaulerContact.NotifyPreferredFormat = model.NotifyPreferredFormat;
            leaseHaulerContact.IsDispatcher = model.IsDispatcher;

            await _leaseHaulerContactRepository.InsertOrUpdateAndGetIdAsync(leaseHaulerContact);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<EditLeaseHaulerTruckResult> EditLeaseHaulerTruck(LeaseHaulerTruckEditDto model)
        {
            var truck = model.Id.HasValue
                ? await _truckRepository.GetAll()
                    .Include(t => t.LeaseHaulerTruck)
                    .FirstAsync(t => t.Id == model.Id.Value)
            : new Truck();

            if (truck.HaulingCompanyTruckId != null)
            {
                throw new UserFriendlyException(L("CannotEditTruckLinkedToHaulingCompany"));
            }

            if (await _truckRepository.GetAll().AnyAsync(t => t.LeaseHaulerTruck.LeaseHaulerId == model.LeaseHaulerId
                && t.TruckCode == model.TruckCode && t.Id != truck.Id))
            {
                throw new UserFriendlyException(L("TruckCode{0}AlreadyExistsForLeaseHauler", model.TruckCode));
            }

            if (model.AlwaysShowOnSchedule && !model.OfficeId.HasValue)
            {
                throw new UserFriendlyException("Office is required");
            }

            var newVehicleCategory = await _vehicleCategoryRepository.GetAll().Where(x => x.Id == model.VehicleCategoryId).FirstOrDefaultAsync();
            if (newVehicleCategory == null)
            {
                throw new UserFriendlyException("Category is required");
            }
            var oldVehicleCategory = truck.VehicleCategoryId == model.VehicleCategoryId
                ? newVehicleCategory
                : truck.VehicleCategoryId != 0
                    ? await _vehicleCategoryRepository.GetAll().Where(x => x.Id == truck.VehicleCategoryId).FirstOrDefaultAsync()
                    : null;

            if ((!model.Id.HasValue || newVehicleCategory != oldVehicleCategory || truck.LeaseHaulerTruck?.AlwaysShowOnSchedule != model.AlwaysShowOnSchedule) && newVehicleCategory.IsPowered && model.AlwaysShowOnSchedule)
            {
                var currentNumberOfTrucks = await _truckRepository.CountAsync(t => t.LocationId != null && t.VehicleCategory.IsPowered && t.Id != model.Id) + 1;
                var maxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
                if (currentNumberOfTrucks > maxNumberOfTrucks)
                {
                    return new EditLeaseHaulerTruckResult
                    {
                        NeededBiggerNumberOfTrucks = currentNumberOfTrucks
                    };
                }
            }

            truck.TruckCode = model.TruckCode;
            truck.VehicleCategoryId = model.VehicleCategoryId;
            truck.CanPullTrailer = model.CanPullTrailer;
            truck.InactivationDate = model.IsActive ? null : model.InactivationDate;
            truck.DefaultDriverId = model.DefaultDriverId;
            truck.LocationId = model.AlwaysShowOnSchedule ? model.OfficeId : null;

            if (truck.IsActive
                && !model.IsActive
                && model.Id != null)
            {
                if (await TruckHasUpcomingLeaseHaulerRequests(model.Id.Value))
                {
                    throw new UserFriendlyException(L("UnableToInactivateLhTruckWithRequests"));
                }
                if (await TruckHasOpenDispatchesOrOrderLines(model.Id.Value))
                {
                    throw new UserFriendlyException(L("UnableToInactivateLhTruckWithOrdersOrDispatches"));
                }
            }
            truck.IsActive = model.IsActive;

            await _truckRepository.InsertOrUpdateAndGetIdAsync(truck);

            if (model.Id == null)
            {
                await _leaseHaulerTruckRepository.InsertAsync(new LeaseHaulerTruck
                {
                    TruckId = truck.Id,
                    LeaseHaulerId = model.LeaseHaulerId,
                    AlwaysShowOnSchedule = model.AlwaysShowOnSchedule
                });
            }
            else
            {
                truck.LeaseHaulerTruck.AlwaysShowOnSchedule = model.AlwaysShowOnSchedule;
            }

            await _crossTenantOrderSender.SyncMaterialCompanyTrucksIfNeeded(truck.Id);

            return new EditLeaseHaulerTruckResult();
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task EditLeaseHaulerDriver(LeaseHaulerDriverEditDto model)
        {
            var driver = model.Id.HasValue ? await _driverRepository.GetAsync(model.Id.Value) : new Driver();

            if (driver.HaulingCompanyDriverId != null)
            {
                throw new UserFriendlyException(L("CannotEditDriverLinkedToHaulingCompany"));
            }

            driver.FirstName = model.FirstName;
            driver.LastName = model.LastName;
            driver.EmailAddress = model.EmailAddress;
            driver.CellPhoneNumber = model.CellPhoneNumber;
            driver.OrderNotifyPreferredFormat = model.OrderNotifyPreferredFormat;
            driver.IsExternal = true;

            if (!driver.IsInactive
                && !model.DriverIsActive
                && model.Id != null)
            {
                if (await DriverHasUpcomingLeaseHaulerRequests(model.Id.Value))
                {
                    throw new UserFriendlyException(L("UnableToInactivateLhDriverWithRequests"));
                }
            }
            driver.IsInactive = !model.DriverIsActive;

            await _driverRepository.InsertOrUpdateAndGetIdAsync(driver);

            if (model.Id == null)
            {
                await _leaseHaulerDriverRepository.InsertAsync(new LeaseHaulerDriver
                {
                    DriverId = driver.Id,
                    LeaseHaulerId = model.LeaseHaulerId
                });
            }

            if (model.EnableForDriverApplication)
            {
                await CreateOrUpdateUserForLHDriver(driver, model);
            }
            else
            {
                await InactivateUserForLHDriver(driver);
            }

            await _crossTenantOrderSender.SyncMaterialCompanyDriversIfNeeded(driver.Id);
        }

        private async Task<bool> DriverHasUpcomingLeaseHaulerRequests(int driverId)
        {
            var today = await GetToday();
            return await _availableLeaseHaulerTruckRepository.GetAll()
                .AnyAsync(x => x.DriverId == driverId && x.Date >= today);
        }

        private async Task<bool> TruckHasUpcomingLeaseHaulerRequests(int truckId)
        {
            var today = await GetToday();
            return await _availableLeaseHaulerTruckRepository.GetAll()
                    .AnyAsync(x => x.TruckId == truckId && x.Date >= today);
        }

        private async Task<bool> TruckHasOpenDispatchesOrOrderLines(int truckId)
        {
            var today = await GetToday();
            return await _dispatchRepository.GetAll()
                    .AnyAsync(x => x.TruckId == truckId && !Dispatch.ClosedDispatchStatuses.Contains(x.Status))
                || await _orderLineTruckRepository.GetAll()
                    .AnyAsync(x => x.TruckId == truckId && !x.IsDone && !x.OrderLine.IsComplete && x.OrderLine.Order.DeliveryDate >= today);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task DeleteLeaseHauler(EntityDto input)
        {
            if (await _leaseHaulerRepository.GetAll()
                    .Where(x => x.Id == input.Id)
                    .AnyAsync(x => x.LeaseHaulerTrucks.Any() || x.LeaseHaulerDrivers.Any()))
            {
                throw new UserFriendlyException("Cannot delete the Lease Hauler because it has one or more trucks or drivers.");
            }

            var lhContacts = await _leaseHaulerContactRepository.GetAll()
                .Where(lhc => lhc.LeaseHaulerId == input.Id)
                .ToListAsync();

            foreach (var lhContact in lhContacts)
            {
                await _leaseHaulerContactRepository.DeleteAsync(lhContact);
            }

            await _leaseHaulerRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task DeleteLeaseHaulerContact(EntityDto input)
        {
            await _leaseHaulerContactRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task DeleteLeaseHaulerTruck(EntityDto input)
        {
            bool hasDependencies = await _availableLeaseHaulerTruckRepository.GetAll()
                    .AnyAsync(x => x.TruckId == input.Id)
                || await _dispatchRepository.GetAll()
                    .AnyAsync(x => x.TruckId == input.Id)
                || await _orderLineTruckRepository.GetAll()
                    .AnyAsync(x => x.TruckId == input.Id);

            if (hasDependencies)
            {
                throw new UserFriendlyException(L("UnableToDeleteTruckWithAssociatedData"));
            }

            await _truckAppService.DeleteTruck(input);
            await _leaseHaulerTruckRepository.DeleteAsync(x => x.TruckId == input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task DeleteLeaseHaulerDriver(EntityDto input)
        {
            if (await _availableLeaseHaulerTruckRepository.GetAll()
                .AnyAsync(x => x.DriverId == input.Id))
            {
                throw new UserFriendlyException(L("UnableToDeleteDriverWithAssociatedData"));
            }

            await _driverAppService.DeleteDriver(input);
            await _leaseHaulerDriverRepository.DeleteAsync(x => x.DriverId == input.Id);
        }

        private async Task<User> GetUserForLhDriver(Driver driver)
        {
            return await GetUserForLhDriver(driver.EmailAddress, driver.UserId);
        }

        private async Task<User> GetUserForLhDriver(string driverEmail, long? driverUserId)
        {
            User user;
            if (driverEmail.IsNullOrEmpty())
            {
                return null;
            }

            if (driverUserId != null)
            {
                user = await UserManager.GetUserByIdAsync(driverUserId.Value);
                if (user == null) //|| user.EmailAddress.IsNullOrEmpty() || user.EmailAddress?.ToUpper() != driverEmail?.ToUpper())
                {
                    return null;
                }
                return user;
            }

            user = await UserManager.FindByEmailAsync(driverEmail);
            return user;
        }

        private async Task CreateOrUpdateUserForLHDriver(Driver driver, LeaseHaulerDriverEditDto model)
        {
            if (driver.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Driver Email Address is required");
            }

            var newUser = false;
            var user = await GetUserForLhDriver(driver);

            if (user == null)
            {
                newUser = true;
                user = await _userCreatorService.CreateUser(new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        PhoneNumber = driver.CellPhoneNumber,
                        EmailAddress = driver.EmailAddress,
                        Name = driver.FirstName,
                        Surname = driver.LastName,
                        OfficeId = driver.OfficeId,
                        IsActive = model.DriverIsActive,
                        IsLockoutEnabled = true,
                        UserName = driver.EmailAddress.Substring(0, driver.EmailAddress.IndexOf("@")),
                        ShouldChangePasswordOnNextLogin = model.ShouldChangePasswordOnNextLogin,
                        Password = model.Password,
                    },
                    AssignedRoleNames = new[] { StaticRoleNames.Tenants.LeaseHaulerDriver },
                    SendActivationEmail = model.SendActivationEmail,
                    SetRandomPassword = model.SetRandomPassword,
                });
            }
            else
            {
                user.Name = model.FirstName;
                user.Surname = model.LastName;
                user.EmailAddress = model.EmailAddress;
                user.PhoneNumber = model.CellPhoneNumber;
                //user.IsActive = model.DriverIsActive;
                user.ShouldChangePasswordOnNextLogin = model.ShouldChangePasswordOnNextLogin;

                CheckErrors(await UserManager.UpdateAsync(user));

                if (model.SetRandomPassword)
                {
                    var randomPassword = await UserManager.CreateRandomPassword();
                    user.Password = _passwordHasher.HashPassword(user, randomPassword);
                    model.Password = randomPassword;
                }
                else if (!model.Password.IsNullOrEmpty())
                {
                    await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                    CheckErrors(await UserManager.ChangePasswordAsync(user, model.Password));
                }

                var otherDrivers = await _driverRepository.GetAll()
                    .Include(x => x.LeaseHaulerDriver)
                    .Where(x => x.Id != driver.Id && x.UserId == user.Id)
                    .ToListAsync();

                if (model.DriverIsActive)
                {
                    user.IsActive = true;
                    foreach (var otherDriver in otherDrivers)
                    {
                        if (!otherDriver.IsInactive)
                        {
                            otherDriver.IsInactive = true;
                            await _driverInactivatorService.InactivateDriverAsync(otherDriver, otherDriver.LeaseHaulerDriver?.LeaseHaulerId);
                        }
                    }
                }
                else
                {
                    if (!otherDrivers.Any(x => !x.IsInactive))
                    {
                        user.IsActive = false;
                    }
                }

                CheckErrors(await UserManager.UpdateAsync(user));
            }

            if (!await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver))
            {
                await UserManager.AddToRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);
            }

            if (model.SendActivationEmail && !newUser)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    model.Password
                );
            }

            driver.UserId = user.Id;
        }

        private async Task InactivateUserForLHDriver(Driver driver)
        {
            if (driver.UserId == null)
            {
                return;
            }

            var user = await GetUserForLhDriver(driver);
            if (user != null)
            {
                var otherDrivers = await _driverRepository.GetAll()
                    .Where(x => x.Id != driver.Id && x.UserId == driver.UserId)
                    .ToListAsync();

                if (!otherDrivers.Any(x => !x.IsInactive)) //if there are no other drivers, or all other drivers are inactive too
                {
                    user.IsActive = false;
                    CheckErrors(await UserManager.UpdateAsync(user));
                    await UserManager.RemoveFromRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);
                }
            }

            await _driverUserLinkService.EnsureCanUnlinkAsync(driver);
            driver.UserId = null;
        }
    }
}
