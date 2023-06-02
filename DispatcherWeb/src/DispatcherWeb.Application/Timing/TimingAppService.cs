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
    }
}
