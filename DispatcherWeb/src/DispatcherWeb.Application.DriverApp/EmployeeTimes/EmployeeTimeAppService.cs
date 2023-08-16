using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApp.EmployeeTimes.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.EmployeeTimes
{
    [AbpAuthorize]
    public class EmployeeTimeAppService : DispatcherWebDriverAppAppServiceBase, IEmployeeTimeAppService
    {
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<Driver> _driverRepository;

        public EmployeeTimeAppService(
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<Driver> driverRepository
            )
        {
            _employeeTimeRepository = employeeTimeRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _driverRepository = driverRepository;
        }

        public async Task<IPagedResult<EmployeeTimeDto>> Get(GetInput input)
        {
            var query = _employeeTimeRepository.GetAll()
                .Where(x => x.UserId == Session.UserId)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                .WhereIf(input.TruckId.HasValue, x => x.EquipmentId == input.TruckId)
                .WhereIf(input.StartDateTimeBegin.HasValue, x => x.StartDateTime >= input.StartDateTimeBegin)
                .WhereIf(input.StartDateTimeEnd.HasValue, x => x.StartDateTime <= input.StartDateTimeEnd)
                .WhereIf(input.EndDateTimeBegin.HasValue, x => x.EndDateTime >= input.EndDateTimeBegin)
                .WhereIf(input.EndDateTimeEnd.HasValue, x => x.EndDateTime <= input.EndDateTimeEnd)
                .WhereIf(input.HasEndTime == true, x => x.EndDateTime != null)
                .WhereIf(input.HasEndTime == false, x => x.EndDateTime == null)
                .WhereIf(input.IsImported.HasValue, x => x.IsImported == input.IsImported)
                .WhereIf(input.ModifiedAfterDateTime.HasValue, d => d.CreationTime > input.ModifiedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.ModifiedAfterDateTime.Value))
                .Select(x => new EmployeeTimeDto
                {
                    Id = x.Id,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    Description = x.Description,
                    TruckId = x.EquipmentId,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    TimeClassificationId = x.TimeClassificationId,
                    LastModifiedDateTime = x.LastModificationTime.HasValue && x.LastModificationTime.Value > x.CreationTime ? x.LastModificationTime.Value : x.CreationTime,
                    IsEditable = x.PayStatementTime == null,
                    IsImported = x.IsImported
                });

            var totalCount = await query.CountAsync();
            var items = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EmployeeTimeDto>(
                totalCount,
                items);
        }

        private async Task EnsureIsEditable(int id)
        {
            if (id == 0)
            {
                return;
            }
            if (await _employeeTimeRepository.GetAll().AnyAsync(x => x.Id == id && x.PayStatementTime != null))
            {
                throw new UserFriendlyException("This EmployeeTime was already added to a pay statement and cannot be edited");
            }
        }

        public async Task<EmployeeTimeDto> Post(EmployeeTimeDto model)
        {
            var employeeTime = model.Id == 0 ? new Drivers.EmployeeTime() : await _employeeTimeRepository.GetAsync(model.Id);
            if (employeeTime == null)
            {
                throw new UserFriendlyException($"EmployeeTime with id ${model.Id} wasn't found");
            }

            await EnsureIsEditable(model.Id);

            var driver = await _driverRepository.GetAll()
                .Where(x => x.UserId == Session.UserId)
                .Select(x => new
                {
                    x.Id,
                    x.IsInactive
                })
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync();

            if (!await _timeClassificationRepository.GetAll().AnyAsync(x => x.Id == model.TimeClassificationId))
            {
                throw new UserFriendlyException($"Time Classification with id {model.TimeClassificationId} wasn't found");
            }

            //should we limit them to editing only their own records?
            //if (model.Id != 0 && employeeTime.UserId != Session.UserId || model.UserId != Session.UserId)
            //{
            //    throw new UserFriendlyException("You can only edit your own time records");
            //}

            if (model.StartDateTime < DispatcherWebConsts.MinDateTime
                || model.EndDateTime < DispatcherWebConsts.MinDateTime)
            {
                throw new UserFriendlyException(L("DateShouldBeAfter2000"));
            }

            var timeClassificationId = await GetValidatedTimeClassificationIdOrNullAsync(model.TimeClassificationId)
                ?? await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, Session.GetTenantId());

            employeeTime.StartDateTime = model.StartDateTime;
            employeeTime.EndDateTime = model.EndDateTime;
            employeeTime.UserId = Session.GetUserId();
            employeeTime.DriverId = driver?.Id;
            employeeTime.Description = model.Description;
            employeeTime.EquipmentId = model.TruckId;
            employeeTime.Latitude = model.Latitude;
            employeeTime.Longitude = model.Longitude;
            employeeTime.TimeClassificationId = timeClassificationId;

            if (model.Id == 0)
            {
                _employeeTimeRepository.Insert(employeeTime);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            model.Id = employeeTime.Id;

            return (await Get(new GetInput { Id = model.Id })).Items.FirstOrDefault();
        }

        private async Task<int?> GetValidatedTimeClassificationIdOrNullAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }
            var timeClassification = await _timeClassificationRepository.GetAll()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id
                }).FirstOrDefaultAsync();

            return timeClassification?.Id;
        }

        public async Task Delete(int id)
        {
            await EnsureIsEditable(id);
            await _employeeTimeRepository.DeleteAsync(id);
        }
    }
}
