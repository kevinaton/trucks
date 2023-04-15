using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
    public class LeaseHaulerRequestEditAppService : DispatcherWebAppServiceBase, ILeaseHaulerRequestEditAppService
    {
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly RoleManager _roleManager;

        public LeaseHaulerRequestEditAppService(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Ticket> ticketRepository,
            INotificationPublisher notificationPublisher,
            RoleManager roleManager
        )
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _ticketRepository = ticketRepository;
            _notificationPublisher = notificationPublisher;
            _roleManager = roleManager;
        }

        public async Task<LeaseHaulerRequestEditDto> GetLeaseHaulerRequestEditDto(int? leaseHaulerRequestId, DateTime? scheduleDate, bool? requestFromScheduler)
        {
            var model = leaseHaulerRequestId != null ? await _leaseHaulerRequestRepository.GetAll()
                .Where(lhr => lhr.Id == leaseHaulerRequestId)
                .Select(lhr => new LeaseHaulerRequestEditDto()
                {
                    Id = lhr.Id,
                    Date = lhr.Date,
                    Shift = lhr.Shift,
                    OfficeId = lhr.OfficeId,
                    LeaseHaulerId = lhr.LeaseHaulerId,
                    LeaseHaulerName = lhr.LeaseHauler.Name,
                    Available = lhr.Available,
                    Approved = lhr.Approved,
                    Comments = lhr.Comments,
                    RequestFromScheduler = requestFromScheduler ?? false
                })
                .FirstAsync()
                : new LeaseHaulerRequestEditDto() { Date = scheduleDate, RequestFromScheduler = requestFromScheduler ?? false };

            if (model.Id != 0)
            {
                model.Trucks = await _availableLeaseHaulerTruckRepository.GetAll()
                    .Where(x => x.Date == model.Date
                        && x.Shift == model.Shift
                        && x.LeaseHaulerId == model.LeaseHaulerId
                        && x.OfficeId == model.OfficeId)
                    .Select(x => new AvailableTrucksTruckEditDto
                    {
                        TruckId = x.TruckId,
                        TruckCode = x.Truck.TruckCode,
                        DriverId = x.DriverId,
                        DriverName = x.Driver.FirstName + " " + x.Driver.LastName
                    }).ToListAsync();

                var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
                {
                    TruckIds = model.Trucks.Where(x => x.TruckId.HasValue).Select(x => x.TruckId.Value).Distinct().ToList(),
                    DriverIds = model.Trucks.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value).Distinct().ToList()
                }.FillFrom(model));

                MergeTrucksInUseIntoAvailableTrucks(model.Trucks, trucksInUse);
            }

            return model;
        }

        private void MergeTrucksInUseIntoAvailableTrucks(List<AvailableTrucksTruckEditDto> truckModels, List<AvailableTruckUsageDto> trucksInUse)
        {
            foreach (var truckModel in truckModels)
            {
                foreach (var truckInUse in trucksInUse)
                {
                    truckModel.IsTruckInUse |= truckInUse.TruckId == truckModel.TruckId;
                    truckModel.IsDriverInUse |= truckInUse.TruckId == truckModel.TruckId && truckInUse.DriverId == truckModel.DriverId;
                    if (truckModel.IsDriverInUse)
                    {
                        break;
                    }
                }
            }
        }

        public async Task<List<AvailableTruckUsageDto>> GetTrucksInUse(GetTrucksInUseInput input)
        {
            input.DriverIds = input.DriverIds ?? new List<int>();
            input.TruckIds = input.TruckIds ?? new List<int>();

            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date
                    && x.OrderLine.Order.Shift == input.Shift
                    && x.OrderLine.Order.LocationId == input.OfficeId)
                .WhereIf(input.TruckIds.Any(),
                    x => input.TruckIds.Contains(x.TruckId))
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.TruckId
                })
                .ToListAsync();

            var dispatches = await _dispatchRepository.GetAll()
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date
                    && x.OrderLine.Order.Shift == input.Shift
                    && x.OrderLine.Order.LocationId == input.OfficeId
                    && !Dispatch.ClosedDispatchStatuses.Contains(x.Status))
                .WhereIf(input.DriverIds.Any() || input.TruckIds.Any(),
                    x => input.TruckIds.Contains(x.TruckId) || input.DriverIds.Contains(x.DriverId))
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.TruckId,
                    DriverId = x.DriverId
                })
                .ToListAsync();

            var tickets = await _ticketRepository.GetAll()
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date
                        && x.OrderLine.Order.Shift == input.Shift
                        && x.OrderLine.Order.LocationId == input.OfficeId
                        && x.TruckId.HasValue)
                .WhereIf(input.DriverIds.Any() || input.TruckIds.Any(),
                    x => input.TruckIds.Contains(x.TruckId.Value)
                            || x.DriverId.HasValue && input.DriverIds.Contains(x.DriverId.Value))
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.TruckId.Value,
                    DriverId = x.DriverId,
                })
                .ToListAsync();

            return orderLineTrucks
                .Union(dispatches)
                .Union(tickets)
                .GroupBy(x => new { x.TruckId, x.DriverId })
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.Key.TruckId,
                    DriverId = x.Key.DriverId
                }).ToList();
        }

        public async Task<LeaseHaulerRequestEditModel> EditLeaseHaulerRequest(LeaseHaulerRequestEditModel model)
        {
            var leaseHaulerRequest = model.Id != 0 ?
                await _leaseHaulerRequestRepository.GetAll().Where(lhr => lhr.Id == model.Id).FirstAsync() :
                new LeaseHaulerRequest { Guid = Guid.NewGuid() };

            EnsureApprovedIsNotGreaterThanAvailable(model.Approved, model.Available);

            leaseHaulerRequest.OfficeId = OfficeId;
            leaseHaulerRequest.Date = model.Date.Date;
            leaseHaulerRequest.Shift = model.Shift;
            leaseHaulerRequest.LeaseHaulerId = model.LeaseHaulerId;
            leaseHaulerRequest.Available = model.Available;
            leaseHaulerRequest.Approved = model.Approved;

            model.Id = await _leaseHaulerRequestRepository.InsertOrUpdateAndGetIdAsync(leaseHaulerRequest);

            model.Trucks = model.Trucks ?? new List<int?>();
            model.Drivers = model.Drivers ?? new List<int?>();
            await UpdateAvailableTrucksAsync(leaseHaulerRequest, model.Trucks, model.Drivers, required: false);

            return model;
        }

        private void EnsureApprovedIsNotGreaterThanAvailable(int? approved, int? available)
        {
            if (approved > available || approved != null && available == null)
            {
                throw new ArgumentException("Approved must be less than or equal to Available!");
            }
        }

        public async Task UpdateAvailable(IdValueInput<int?> input)
        {
            var leaseHaulerRequest = await _leaseHaulerRequestRepository.GetAsync(input.Id);
            EnsureApprovedIsNotGreaterThanAvailable(leaseHaulerRequest.Approved, input.Value);
            leaseHaulerRequest.Available = input.Value;
        }

        public async Task UpdateApproved(IdValueInput<int?> input)
        {
            var leaseHaulerRequest = await _leaseHaulerRequestRepository.GetAsync(input.Id);
            EnsureApprovedIsNotGreaterThanAvailable(input.Value, leaseHaulerRequest.Available);
            if (!leaseHaulerRequest.Approved.HasValue || input.Value >= leaseHaulerRequest.Approved)
            {
                leaseHaulerRequest.Approved = input.Value;
                return;
            }
            var availableTruckCount = await _availableLeaseHaulerTruckRepository.GetAll()
                .CountAsync(x => x.Date == leaseHaulerRequest.Date
                            && x.LeaseHaulerId == leaseHaulerRequest.LeaseHaulerId
                            && x.Shift == leaseHaulerRequest.Shift);
            if (availableTruckCount == 0 || availableTruckCount <= input.Value)
            {
                leaseHaulerRequest.Approved = input.Value;
                return;
            }
            throw new UserFriendlyException("Approved must be greater than or equal to the number of already added trucks");
        }

        [AbpAllowAnonymous]
        public async Task<AvailableTrucksEditDto> GetAvailableTrucksEditDto(Guid leaseHaulerRequestGuid)
        {
            var model = await _leaseHaulerRequestRepository.GetAll()
                .Where(lhr => lhr.Guid == leaseHaulerRequestGuid)
                .Select(lhr => new AvailableTrucksEditDto()
                {
                    Id = lhr.Id,
                    TenantId = lhr.TenantId,
                    LeaseHaulerId = lhr.LeaseHaulerId,
                    OfficeId = lhr.OfficeId,
                    Date = lhr.Date,
                    Shift = lhr.Shift,
                    Available = lhr.Available,
                    Approved = lhr.Approved,
                    Scheduled = _orderLineTruckRepository.GetAll()
                        .Where(olt => olt.OrderLine.Order.LocationId == lhr.OfficeId
                            && olt.OrderLine.Order.Shift == lhr.Shift
                            && olt.OrderLine.Order.DeliveryDate == lhr.Date
                            && olt.Truck.LeaseHaulerTruck.LeaseHaulerId == lhr.LeaseHaulerId)
                        .Count(),
                    Comments = lhr.Comments
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return null;
            }

            model.CustomerName = await SettingManager.GetSettingValueForTenantAsync(AppSettings.General.CompanyName, model.TenantId);
            model.ShiftName = await SettingManager.GetShiftName(model.Shift, model.TenantId);

            var timeZone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, model.TenantId);
            var expirationDateTime = model.Date.Date.AddDays(2).ConvertTimeZoneFrom(timeZone);
            model.IsExpired = Clock.Now >= expirationDateTime;

            if (model.ShowTruckControls)
            {
                model.Trucks = await _availableLeaseHaulerTruckRepository.GetAll()
                    .Where(x => x.Date == model.Date
                        && x.Shift == model.Shift
                        && x.LeaseHaulerId == model.LeaseHaulerId
                        && x.OfficeId == model.OfficeId)
                    .Select(x => new AvailableTrucksTruckEditDto
                    {
                        TruckId = x.TruckId,
                        TruckCode = x.Truck.TruckCode,
                        DriverId = x.DriverId,
                        DriverName = x.Driver.FirstName + " " + x.Driver.LastName
                    }).ToListAsync();

                var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
                {
                    TruckIds = model.Trucks.Where(x => x.TruckId.HasValue).Select(x => x.TruckId.Value).Distinct().ToList(),
                    DriverIds = model.Trucks.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value).Distinct().ToList()
                }.FillFrom(model));

                MergeTrucksInUseIntoAvailableTrucks(model.Trucks, trucksInUse);
            }

            return model;
        }

        [AbpAllowAnonymous]
        public async Task EditAvailableTrucks(AvailableTrucksEditModel model)
        {
            ShortGuid.TryParse(model.Id, out var guid);
            var leaseHaulerRequest = await _leaseHaulerRequestRepository.FirstOrDefaultAsync(x => x.Guid == guid.Guid);

            if (leaseHaulerRequest == null)
            {
                throw new ArgumentException($"There is no LeaseHaulerRequest with Guid={guid.Guid}");
            }

            if (leaseHaulerRequest.Approved.HasValue && model.Available < leaseHaulerRequest.Approved)
            {
                throw new UserFriendlyException("You can't reduce the number of trucks available below the already approved value");
            }

            if (leaseHaulerRequest.Available.HasValue && leaseHaulerRequest.Available != model.Available)
            {
                await NotifyDispatchersAboutChangedAvailableTrucksNumber(leaseHaulerRequest, model.Available);
            }

            leaseHaulerRequest.Available = model.Available;

            if (leaseHaulerRequest.Approved > 0)
            {
                //the field is only shown when 'approved' was set
                leaseHaulerRequest.Comments = model.Comments;

                await UpdateAvailableTrucksAsync(leaseHaulerRequest, model.Trucks, model.Drivers, required: true);
            }

            // Local functions
            // #8604 Commented until further notice
            //void ThrowUserFriendlyExceptionToRefreshPageIfAvailableHasChanged()
            //{
            //    (
            //        leaseHaulerRequest.Available != model.Available ||
            //        leaseHaulerRequest.Approved != model.Approved
            //    ).ThrowUserFriendlyExceptionIfTrue("The Available or Approved values are changed. Please refresh the page.");
            //}
        }

        private async Task UpdateAvailableTrucksAsync(LeaseHaulerRequest leaseHaulerRequest, List<int?> trucks, List<int?> drivers, bool required)
        {
            var existingTrucks = await _availableLeaseHaulerTruckRepository.GetAll()
                    .Where(x => x.Date == leaseHaulerRequest.Date
                        && x.Shift == leaseHaulerRequest.Shift
                        && x.LeaseHaulerId == leaseHaulerRequest.LeaseHaulerId
                        && x.OfficeId == leaseHaulerRequest.OfficeId)
                    .ToListAsync();

            var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
            {
                TruckIds = existingTrucks.Select(x => x.TruckId).ToList(),
                DriverIds = existingTrucks.Select(x => x.DriverId).Distinct().ToList(),
                Date = leaseHaulerRequest.Date,
                LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                OfficeId = leaseHaulerRequest.OfficeId,
                Shift = leaseHaulerRequest.Shift
            });

            if (required)
            {
                if (trucks == null
                    || drivers == null
                    || trucks.Any(x => !x.HasValue)
                    || drivers.Any(x => !x.HasValue))
                {
                    throw new UserFriendlyException("Trucks and Drivers are required fields");
                }
            }

            var trucksWithValues = trucks?.Where(x => x.HasValue).ToList();

            if (trucksWithValues?.Distinct().Count() < trucksWithValues?.Count)
            {
                throw new UserFriendlyException("You specified the same truck more than once");
            }

            if (trucks != null && drivers != null)
            {
                for (int i = 0; i < Math.Min(trucks.Count, drivers.Count); i++)
                {
                    if (!trucks[i].HasValue)
                    {
                        if (drivers[i].HasValue)
                        {
                            throw new UserFriendlyException("Truck is required for selected Drivers");
                        }
                        continue;
                    }
                    if (!drivers[i].HasValue)
                    {
                        throw new UserFriendlyException("Driver is required for selected Trucks");
                    }
                    var existing = existingTrucks.FirstOrDefault(x => x.TruckId == trucks[i]);
                    if (existing != null)
                    {
                        if (existing.DriverId != drivers[i].Value)
                        {
                            if (trucksInUse.Any(x => x.TruckId == existing.TruckId && x.DriverId == existing.DriverId))
                            {
                                throw new UserFriendlyException("One of the drivers is associated with dispatches, or tickets.", "If you want to remove or change the driver, you need to remove any associated dispatches, and tickets for this date.");
                            }
                            existing.DriverId = drivers[i].Value;
                        }
                    }
                    else
                    {
                        await _availableLeaseHaulerTruckRepository.InsertAsync(new AvailableLeaseHaulerTruck
                        {
                            TenantId = leaseHaulerRequest.TenantId,
                            LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                            OfficeId = leaseHaulerRequest.OfficeId,
                            Date = leaseHaulerRequest.Date,
                            Shift = leaseHaulerRequest.Shift,
                            TruckId = trucks[i].Value,
                            DriverId = drivers[i].Value
                        });
                    }
                }
            }

            var trucksToDelete = existingTrucks.Where(x => trucks == null || !trucks.Contains(x.TruckId)).ToList();
            foreach (var truckToDelete in trucksToDelete)
            {
                if (trucksInUse.Any(x => x.TruckId == truckToDelete.TruckId))
                {
                    throw new UserFriendlyException("One of the trucks is associated with orders, dispatches, or tickets.", "If you want to delete this record, you need to remove any associated orders, dispatches, and tickets for this date.");
                }

                await _availableLeaseHaulerTruckRepository.DeleteAsync(truckToDelete);
            }
        }

        private async Task NotifyDispatchersAboutChangedAvailableTrucksNumber(LeaseHaulerRequest leaseHaulerRequest, int newValue)
        {
            var userIds = new List<Abp.UserIdentifier>();

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var dispatchingRoleIds = _roleManager.Roles
                    .Where(x => x.TenantId == leaseHaulerRequest.TenantId
                        && x.Name == StaticRoleNames.Tenants.Dispatching)
                    .Select(x => x.Id)
                    .ToList();

                var users = await UserManager.Users
                    .Include(x => x.Roles)
                    .Where(x => x.OfficeId == leaseHaulerRequest.OfficeId
                        && x.TenantId == leaseHaulerRequest.TenantId
                        && x.Roles.Any(r => dispatchingRoleIds.Contains(r.RoleId)))
                    .ToListAsync();

                foreach (var user in users)
                {
                    userIds.Add(user.ToUserIdentifier());
                }
            }

            if (userIds.Any())
            {
                var additionalData = await _leaseHaulerRequestRepository.GetAll()
                    .Where(x => x.Id == leaseHaulerRequest.Id)
                    .Select(x => new
                    {
                        LeaseHaulerName = x.LeaseHauler.Name
                    }).FirstAsync();

                var notificationData = new MessageNotificationData(
                    $"Number of trucks available has been changed from {leaseHaulerRequest.Available} to {newValue} by {additionalData.LeaseHaulerName} for {leaseHaulerRequest.Date.ToShortDateString()}{(leaseHaulerRequest.Shift.HasValue ? " " + await SettingManager.GetShiftName(leaseHaulerRequest.Shift) : "")}."
                )
                {
                    ["leaseHaulerRequestId"] = leaseHaulerRequest.Id
                };

                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.AvailableTruckNumberChanged,
                    notificationData,
                    severity: NotificationSeverity.Info,
                    userIds: userIds.ToArray()
                );
            }
        }
    }
}
