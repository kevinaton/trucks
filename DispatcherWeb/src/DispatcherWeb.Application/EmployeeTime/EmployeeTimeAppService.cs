using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;
using DispatcherWeb.EmployeeTime.Exporting;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SyncRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EmployeeTime
{
    [AbpAuthorize]
    public class EmployeeTimeAppService : DispatcherWebAppServiceBase, IEmployeeTimeAppService
    {
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IEmployeeTimeListCsvExporter _employeeTimeListCsvExporter;
        private readonly ISyncRequestSender _syncRequestSender;

        public EmployeeTimeAppService(
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<Driver> driverRepository,
            IEmployeeTimeListCsvExporter employeeTimeListCsvExporter,
            ISyncRequestSender syncRequestSender
                )
        {
            _employeeTimeRepository = employeeTimeRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _driverRepository = driverRepository;
            _employeeTimeListCsvExporter = employeeTimeListCsvExporter;
            _syncRequestSender = syncRequestSender;
        }

        private async Task<IOrderedQueryable<EmployeeTimeDto>> GetEmployeeTimeRecordsQueryAsync(GetEmployeeTimeRecordsInput input)
        {
            var indexModel = await GetEmployeeTimeIndexModel();

            var timezone = await GetTimezone();
            input.StartDateStart = input.StartDateStart?.ConvertTimeZoneFrom(timezone);
            input.StartDateEnd = input.StartDateEnd?.AddDays(1).ConvertTimeZoneFrom(timezone);

            var query = _employeeTimeRepository.GetAll()
                .WhereIf(input.StartDateStart.HasValue, x => x.StartDateTime >= input.StartDateStart.Value)
                .WhereIf(input.StartDateEnd.HasValue, x => x.StartDateTime < input.StartDateEnd.Value)
                .WhereIf(indexModel.LockToCurrentUser, x => x.UserId == indexModel.UserId)
                .WhereIf(!indexModel.LockToCurrentUser && input.EmployeeId.HasValue, x => x.UserId == input.EmployeeId)
                .WhereIf(input.TimeClassificationId.HasValue, x => x.TimeClassificationId == input.TimeClassificationId)
                .Select(x => new EmployeeTimeDto
                {
                    Id = x.Id,
                    EmployeeName = x.User.Surname + ", " + x.User.Name,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    ManualHourAmount = x.ManualHourAmount,
                    ElapsedHoursSort = x.ManualHourAmount.HasValue ? (int)x.ManualHourAmount.Value * 3600 : EF.Functions.DateDiffSecond(x.StartDateTime, x.EndDateTime),
                    TimeClassificationName = x.TimeClassification.Name
                })
                .OrderBy(input.Sorting);

            return query;
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry)]
        public async Task<PagedResultDto<EmployeeTimeDto>> GetEmployeeTimeRecords(GetEmployeeTimeRecordsInput input)
        {
            var query = await GetEmployeeTimeRecordsQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EmployeeTimeDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry)]
        [HttpPost]
        public async Task<FileDto> GetEmployeeTimeRecordsToCsv(GetEmployeeTimeRecordsInput input)
        {
            var query = await GetEmployeeTimeRecordsQueryAsync(input);
            var items = await query
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            var timezone = await GetTimezone();
            items.ForEach(x =>
            {
                x.StartDateTime = x.StartDateTime.ConvertTimeZoneTo(timezone);
                x.EndDateTime = x.EndDateTime?.ConvertTimeZoneTo(timezone);
            });

            return _employeeTimeListCsvExporter.ExportToFile(items);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditAll, AppPermissions.Pages_TimeEntry_EditPersonal)]
        public async Task<EmployeeTimeEditDto> GetEmployeeTimeForEdit(NullableIdDto input)
        {
            var indexModel = await GetEmployeeTimeIndexModel();
            EmployeeTimeEditDto employeeTimeEditDto;

            if (input.Id.HasValue)
            {
                employeeTimeEditDto = await _employeeTimeRepository.GetAll()
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new EmployeeTimeEditDto
                    {
                        Id = x.Id,
                        EmployeeId = x.UserId,
                        EmployeeName = x.User.Surname + ", " + x.User.Name,
                        StartDateTime = x.StartDateTime,
                        EndDateTime = x.EndDateTime,
                        ManualHourAmount = x.ManualHourAmount,
                        Description = x.Description,
                        TimeClassificationId = x.TimeClassificationId,
                        TimeClassificationName = x.TimeClassification.Name,
                        TimeOffId = x.TimeOffId,
                        //DriverPayRate = x.PayRate
                    }).FirstAsync();

                if (indexModel.LockToCurrentUser && employeeTimeEditDto.EmployeeId != indexModel.UserId)
                {
                    throw new UserFriendlyException("You do not have permission to edit time records of other users");
                }

                var driverId = await _driverRepository.GetDriverIdByUserIdOrDefault(employeeTimeEditDto.EmployeeId);
                employeeTimeEditDto.TimeClassificationAllowsManualTime = driverId == 0 ? true : await _employeeTimeClassificationRepository.GetAll()
                        .Where(x => x.DriverId == driverId && x.TimeClassificationId == employeeTimeEditDto.TimeClassificationId)
                        .Select(e => e.AllowForManualTime)
                        .FirstOrDefaultAsync();
            }
            else
            {
                employeeTimeEditDto = new EmployeeTimeEditDto
                {
                    LockToCurrentUser = indexModel.LockToCurrentUser,
                    EmployeeId = indexModel.UserId ?? 0,
                    EmployeeName = indexModel.UserFullName
                };
            }

            var timezone = await GetTimezone();
            employeeTimeEditDto.StartDateTime = employeeTimeEditDto.StartDateTime?.ConvertTimeZoneTo(timezone);
            employeeTimeEditDto.EndDateTime = employeeTimeEditDto.EndDateTime?.ConvertTimeZoneTo(timezone);
            employeeTimeEditDto.LockToCurrentUser = indexModel.LockToCurrentUser;

            return employeeTimeEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditAll, AppPermissions.Pages_TimeEntry_EditPersonal)]
        public async Task EditEmployeeTime(EmployeeTimeEditDto model)
        {
            var indexModel = await GetEmployeeTimeIndexModel();
            await CreateOrUpdateEmployeeTime(model, indexModel);
        }

        private async Task CreateOrUpdateEmployeeTime(EmployeeTimeEditDto model, EmployeeTimeIndexDto indexModel)
        {
            var entity = model.Id.HasValue
                ? await _employeeTimeRepository.GetAll()
                    .Include(x => x.PayStatementTime)
                    .Where(x => x.Id == model.Id.Value)
                    .FirstAsync()
                : new Drivers.EmployeeTime();

            if (indexModel.LockToCurrentUser && indexModel.UserId.HasValue)
            {
                if (entity.Id == 0)
                {
                    entity.UserId = indexModel.UserId.Value;
                }
                else if (entity.UserId != indexModel.UserId)
                {
                    throw new UserFriendlyException("You do not have permission to edit time records of other users");
                }
            }
            else
            {
                entity.UserId = model.EmployeeId;
            }

            if (entity.PayStatementTime != null)
            {
                throw new UserFriendlyException(L("UnableToEditEmployeeTimeWithAssociatedData"));
            }

            var timezone = await GetTimezone();
            if (!model.StartDateTime.HasValue)
            {
                throw new UserFriendlyException("StartDateTime is required");
            }

            entity.ManualHourAmount = model.ManualHourAmount;
            entity.StartDateTime = model.StartDateTime.Value.ConvertTimeZoneFrom(timezone);
            entity.EndDateTime = model.ManualHourAmount.HasValue ? entity.StartDateTime : model.EndDateTime?.ConvertTimeZoneFrom(timezone);
            if (entity.StartDateTime < DispatcherWebConsts.MinDateTime
                || entity.EndDateTime < DispatcherWebConsts.MinDateTime)
            {
                throw new UserFriendlyException(L("DateShouldBeAfter2000"));
            }
            var timeClassificationHasChanged = entity.TimeClassificationId != model.TimeClassificationId;
            entity.TimeClassificationId = model.TimeClassificationId;
            entity.Description = model.Description;
            entity.TenantId = Session.TenantId ?? 0;
            entity.TimeOffId = model.TimeOffId;

            var driver = await _driverRepository.GetAll()
                .Where(x => x.UserId == entity.UserId)
                .Select(x => new
                {
                    x.Id,
                    x.IsInactive
                })
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync();

            entity.DriverId = driver?.Id;

            //if (timeClassificationHasChanged)
            //{
            //    var timeClassification = await _employeeTimeClassificationRepository.GetAll()
            //        .Where(x => x.Driver.UserId == entity.UserId && x.TimeClassificationId == entity.TimeClassificationId)
            //        .Select(x => new
            //        {
            //            x.DriverId,
            //            x.TimeClassification.IsProductionBased,
            //            x.PayRate
            //        })
            //        .FirstOrDefaultAsync();

            //    if (timeClassification != null && !timeClassification.IsProductionBased)
            //    {
            //        entity.PayRate = timeClassification.PayRate;
            //    }
            //    else
            //    {
            //        entity.PayRate = null;
            //    }
            //}
            //else
            //{
            //    if (model.DriverPayRate > 0)
            //    {
            //    }
            //}
            //entity.PayRate = model.DriverPayRate;

            await _employeeTimeRepository.InsertOrUpdateAndGetIdAsync(entity);

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.EmployeeTime, entity.ToChangedEntity()));
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditAll, AppPermissions.Pages_TimeEntry_EditPersonal)]
        public async Task DeleteEmployeeTime(EntityDto input)
        {
            var indexModel = await GetEmployeeTimeIndexModel();
            var employeeTime = await _employeeTimeRepository.GetAll()
                .Include(x => x.PayStatementTime)
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            if (indexModel.LockToCurrentUser && indexModel.UserId != employeeTime.UserId)
            {
                throw new UserFriendlyException("You do not have permission to edit time records of other users");
            }

            if (employeeTime.PayStatementTime != null)
            {
                throw new UserFriendlyException(L("UnableToDeleteEmployeeTimeWithAssociatedData"));
            }

            await _employeeTimeRepository.DeleteAsync(employeeTime);

            await CurrentUnitOfWork.SaveChangesAsync();
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.EmployeeTime, employeeTime.ToChangedEntity(), ChangeType.Removed));
        }

        public async Task<EmployeeTimeIndexDto> GetEmployeeTimeIndexModel()
        {
            var lockToCurrentUser = !await IsGrantedAsync(AppPermissions.Pages_TimeEntry_EditAll) && await IsGrantedAsync(AppPermissions.Pages_TimeEntry_EditPersonal);

            var user = lockToCurrentUser ? await UserManager.Users
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    x.Id,
                    FullName = x.Surname + ", " + x.Name
                }).FirstAsync() : null;

            return new EmployeeTimeIndexDto
            {
                UserId = user?.Id,
                UserFullName = user?.FullName,
                LockToCurrentUser = lockToCurrentUser
            };
        }

        public async Task<PagedResultDto<SelectListDto>> GetUsersSelectList(GetSelectListInput input)
        {
            var query = _driverRepository.GetAll()
                .Where(x => x.User.IsActive && x.OfficeId != null)
                .Select(x => new SelectListDto
                {
                    Id = x.UserId.ToString(),
                    Name = x.LastName + ", " + x.FirstName
                });

            return await query.GetSelectListResult(input);
        }

        public async Task AddBulkTime(AddBulkTimeDto addBulkTimeDto)
        {
            var indexModel = await GetEmployeeTimeIndexModel();
            var selectedDriverIds = addBulkTimeDto.DriverIds.AsEnumerable();
            var drivers = await _driverRepository.GetAll()
                            .Where(driver => selectedDriverIds.Contains(driver.Id) && driver.UserId.HasValue)
                            .Select(driver => new
                            {
                                DriverId = driver.Id,
                                driver.UserId
                            })
                            .ToListAsync();

            drivers.ForEach(driver =>
            {
                var empTimeEdit = new EmployeeTimeEditDto()
                {
                    EmployeeId = driver.UserId.Value,
                    Description = addBulkTimeDto.Description,
                    StartDateTime = addBulkTimeDto.StartDateTime,
                    EndDateTime = addBulkTimeDto.EndDateTime,
                    TimeClassificationId = addBulkTimeDto.TimeClassificationId.Value
                };
                CreateOrUpdateEmployeeTime(empTimeEdit, indexModel).Wait();
            });
        }
    }
}
