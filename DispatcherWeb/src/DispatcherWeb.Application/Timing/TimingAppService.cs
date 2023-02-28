using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Drivers;
using DispatcherWeb.LuckStone;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Timing.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Timing
{
    public class TimingAppService : DispatcherWebAppServiceBase, ITimingAppService
    {
        private readonly ITimeZoneService _timeZoneService;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<LuckStoneEarnings> _luckStoneEarningsRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;

        public TimingAppService(
            ITimeZoneService timeZoneService,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<LuckStoneEarnings> luckStoneEarningsRepository,
            IRepository<Office> officeRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository
            )
        {
            _timeZoneService = timeZoneService;
            _driverAssignmentRepository = driverAssignmentRepository;
            _luckStoneEarningsRepository = luckStoneEarningsRepository;
            _officeRepository = officeRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
        }

        public async Task<ListResultDto<NameValueDto>> GetTimezones(GetTimezonesInput input)
        {
            var timeZones = await GetTimezoneInfos(input.DefaultTimezoneScope);
            return new ListResultDto<NameValueDto>(timeZones);
        }

        public async Task<List<ComboboxItemDto>> GetTimezoneComboboxItems(GetTimezoneComboboxItemsInput input)
        {
            var timeZones = await GetTimezoneInfos(input.DefaultTimezoneScope);
            var timeZoneItems = new ListResultDto<ComboboxItemDto>(timeZones.Select(e => new ComboboxItemDto(e.Value, e.Name)).ToList()).Items.ToList();

            if (!string.IsNullOrEmpty(input.SelectedTimezoneId))
            {
                var selectedEdition = timeZoneItems.FirstOrDefault(e => e.Value == input.SelectedTimezoneId);
                if (selectedEdition != null)
                {
                    selectedEdition.IsSelected = true;
                }
            }

            return timeZoneItems;
        }

        private async Task<List<NameValueDto>> GetTimezoneInfos(SettingScopes defaultTimezoneScope)
        {
            var defaultTimezoneId = await _timeZoneService.GetDefaultTimezoneAsync(defaultTimezoneScope, AbpSession.TenantId);
            var defaultTimezoneName = $"{L("Default")} [{defaultTimezoneId}]";

            var timeZones = _timeZoneService.GetWindowsTimezones();

            timeZones.Insert(0, new NameValueDto(defaultTimezoneName, string.Empty));
            return timeZones;
        }

        [Obsolete("We'll use this to convert the timezones once. The ObsoleteAttribute was added to suppress obsolete warnings for those obsolete time fields")]
        [UnitOfWork(false)]
        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
        public async Task ConvertTimesToUtc(int maxRowsPerBatch)
        {
            if (maxRowsPerBatch <= 0)
            {
                throw new UserFriendlyException("maxRowsPerBatch parameter is required, try 1000");
            }

            var tenants = await TenantManager.Tenants.Select(x => x.Id).ToListAsync();

            foreach (var tenantId in tenants)
            {
                var timezone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, tenantId);

                //Driver Assignments
                var updatedRowCount = 0;
                do
                {
                    using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var driverAssignments = await _driverAssignmentRepository.GetAll()
                            .Where(x => x.StartTime == null && x.StartTimeObsolete != null)
                            .OrderBy(x => x.Id)
                            .Take(maxRowsPerBatch)
                            .ToListAsync();

                        foreach (var driverAssignment in driverAssignments)
                        {
                            driverAssignment.StartTime = driverAssignment.StartTimeObsolete?.ConvertTimeZoneFrom(timezone);
                        }

                        updatedRowCount = driverAssignments.Count;
                        uow.Complete();
                    }
                } while (updatedRowCount > 0);


                //LuckStoneEarnings
                do
                {
                    using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var luckStoneEarnings = await _luckStoneEarningsRepository.GetAll()
                            .Where(x => !x.TicketDateTimeWasConverted)
                            .OrderBy(x => x.Id)
                            .Take(maxRowsPerBatch)
                            .ToListAsync();

                        foreach (var luckStoneEarning in luckStoneEarnings)
                        {
                            luckStoneEarning.TicketDateTime = luckStoneEarning.TicketDateTimeObsolete.ConvertTimeZoneFrom(timezone);
                            luckStoneEarning.TicketDateTimeWasConverted = true;
                        }

                        updatedRowCount = luckStoneEarnings.Count;
                        uow.Complete();
                    }
                } while (updatedRowCount > 0);

                //Office
                do
                {
                    using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var offices = await _officeRepository.GetAll()
                            .Where(x => x.DefaultStartTime == null && x.DefaultStartTimeObsolete != null)
                            .OrderBy(x => x.Id)
                            .Take(maxRowsPerBatch)
                            .ToListAsync();

                        foreach (var office in offices)
                        {
                            office.DefaultStartTime = office.DefaultStartTimeObsolete?.ConvertTimeZoneFrom(timezone);
                        }

                        updatedRowCount = offices.Count;
                        uow.Complete();
                    }
                } while (updatedRowCount > 0);

                //OrderLine
                var lastUpdatedId = 0;
                do
                {
                    using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var orderLines = await _orderLineRepository.GetAll()
                            .Where(x => x.Id > lastUpdatedId &&
                                (x.TimeOnJob == null && x.TimeOnJobObsolete != null
                                || x.FirstStaggeredTimeOnJob == null && x.FirstStaggeredTimeOnJobObsolete != null))
                            .OrderBy(x => x.Id)
                            .Take(maxRowsPerBatch)
                            .ToListAsync();

                        foreach (var orderLine in orderLines)
                        {
                            orderLine.TimeOnJob = orderLine.TimeOnJobObsolete?.ConvertTimeZoneFrom(timezone);
                            orderLine.FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJobObsolete?.ConvertTimeZoneFrom(timezone);
                        }

                        if (orderLines.Any())
                        {
                            lastUpdatedId = orderLines.Max(x => x.Id);
                        }

                        updatedRowCount = orderLines.Count;
                        uow.Complete();
                    }
                } while (updatedRowCount > 0);

                //OrderLineTruck
                lastUpdatedId = 0;
                do
                {
                    using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                            .Where(x => x.Id > lastUpdatedId &&
                                x.TimeOnJob == null && x.TimeOnJobObsolete != null)
                            .OrderBy(x => x.Id)
                            .Take(maxRowsPerBatch)
                            .ToListAsync();

                        foreach (var orderLineTruck in orderLineTrucks)
                        {
                            orderLineTruck.TimeOnJob = orderLineTruck.TimeOnJobObsolete?.ConvertTimeZoneFrom(timezone);
                        }

                        if (orderLineTrucks.Any())
                        {
                            lastUpdatedId = orderLineTrucks.Max(x => x.Id);
                        }

                        updatedRowCount = orderLineTrucks.Count;
                        uow.Complete();
                    }
                } while (updatedRowCount > 0);

            }
        }
    }
}
